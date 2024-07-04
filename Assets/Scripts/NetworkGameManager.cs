using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager instance;
    public const int MAX_PLAYERS = 12;
    public GameObject playerPrefab;

    public NetworkList<PlayerData> playerDataNetworkList; // 네트워크 상에서 플레이어 데이터를 관리하는 List.
    public delegate void OnPlayerDataListChanged(); // 플레이어 데이터 리스트 변경 시 호출될 델리게이트 정의.
    public static OnPlayerDataListChanged onPlayerDataListChanged; // 델리게이트 인스턴스.

    public GameObject myPlayer; // only set when ingame, 로컬 플레이어 오브젝트 저장 변수. 

    string username;
    public enum GameState // only allow players to join while waiting to start, 게임 상태 나타내는 열거형 정의.
    {
        WaitingToStart, // 게임 시작 기다리는 상태.
        InHub, // 로비에 있는 상태.
        InGame, // 게임 중.
        End // 게임 끝.
    }
    public NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.WaitingToStart); // 네트워크에서 정의되는 게임 상태 변수 정의, 초기값은 WaitingToStart 상태로.
    private void Awake()
    {
        // 싱글톤 패턴 구현.
        if (NetworkGameManager.instance == null) // instance가 비어있으면
        {
            instance = this; // 현재 인스턴스로 설정.
        }
        else // 그렇지 않으면.
        {
            Destroy(this.gameObject); // 자신을 파괴.
        }
        playerDataNetworkList = new NetworkList<PlayerData>();
        DontDestroyOnLoad(gameObject); // 이 게임 오브젝트가 씬 전환 시 파괴되지 않도록 함.

        username = PlayerPrefs.GetString("USERNAME", "Guest: " + Random.Range(100, 1000)); // PlayerPrefs에 저장된 사용자 이름 불러오고 없으면 무작위.
    }

    private void Start()
    {
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged; // playerDataNetworkList가 변경될 때 호출되는 이벤트 핸들러 등록.
    }

    public string GetUsername()
    {
        return username;
    }
    public void SetUsername(string _username)
    {
        if (string.IsNullOrWhiteSpace(_username)) // 이름이 비어있거나 공백인 경우.
        {
            username = "Guest: " + Random.Range(100, 1000); // 무작위.
        }
        else
        {
             username = _username;
        }

        PlayerPrefs.SetString("USERNAME", username); // PlayerPrefs에 설정된 이름 저장.
    }

    public string GetUsernameFromClientId(ulong _clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList) // playerDataNetworkList의 playerData 순회.
        {
            if (playerData.clientId == _clientId) // 둘이 같으면.
                return playerData.username.ToString(); // 문자열로 유저네임 출력.
        }
        return default;
    }
    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent) // 이벤트 핸들러.
    {
        //Debug.Log("Invoke");
        //Debug.Log(playerDataNetworkList.Count);
        onPlayerDataListChanged?.Invoke(); // 등록된 델리게이트 호출.
    }
    public bool IsPlayerIndexConnected(int playerIndex) // 주어진 인덱스의 플레이어가 연결되어 있는지.
    {
        return playerIndex < playerDataNetworkList.Count;
    }
    public PlayerData GetPlayerDataFromIndex(int _playerIndex) // 인덱스를 통해.
    {
        return playerDataNetworkList[_playerIndex]; // 플레이어 데이터 반환.
    }
    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList) // playerDataNetworkList에서 playerData 순회.
        {
            if (playerData.clientId == clientId) // 둘이 같으면.
                return playerData; // playerData 반환.
        }
        return default;
    }
    public PlayerData GetLocalPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId); // 로컬 클라이언트 플레이어 데이터 반환.
    }

    // 클라이언트 ID를 통해 플레이어 데이터의 인덱스를 반환하는 메서드.
    public int GetPlayerDataIndexFromClientID(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
                return i;
        }
        return -1;
    }

    // 안 씀.
    public int GetPlayerSkinFromIndex(int _playerIndex)
    {
        return playerDataNetworkList[_playerIndex].skinIndex;
    }

    // 안 씀.
    public void ChangePlayerSkin(int skinIndex)
    {
        ChangePlayerSkinServerRpc(skinIndex);
    }

    // 안 씀.
    [ServerRpc(RequireOwnership = false)]
    void ChangePlayerSkinServerRpc(int skinIndex, ServerRpcParams rpcParams = default)
    {
        int playerIndex = GetPlayerDataIndexFromClientID(rpcParams.Receive.SenderClientId);
        PlayerData data = playerDataNetworkList[playerIndex];
        data.skinIndex = skinIndex;
        playerDataNetworkList[playerIndex] = data;
    }

    // 호스트 시작 메서드.
    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += Network_ConnectionApprovalCallback; // 연결 승인 콜백 등록.
        NetworkManager.Singleton.OnClientConnectedCallback += Network_Server_OnClientConnectedCallback; // 클라이언트 연결 콜백 등록.
        NetworkManager.Singleton.OnClientDisconnectCallback += Network_Server_OnClientDisconnectCallback; // 클라이언트 연결 해제 콜백 등록.

        NetworkManager.Singleton.StartHost(); // 호스트 시작.
        LoadLobbyJoinedScene(); // 로비 씬 로드.
    }

    private void Network_Server_OnClientDisconnectCallback(ulong _clientId) // 클라이언트가 연결 끊었을 때 호출되는 콜백.
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData data = playerDataNetworkList[i];
            if (data.clientId == _clientId)
            {
                playerDataNetworkList.RemoveAt(i); // 연결 끊긴 클라이언트의 플레이어 데이터를 리스트에서 제거.
            }
        }

        if (SceneManager.GetActiveScene().name == "MULTIPLAYER_GAME_SCENE")
        {
            //Scoreboard.Instance.ResetScoreboard();
        }
    }
    private void Network_Server_OnClientConnectedCallback(ulong _clientId) // 클라이언트가 연결되었을 때 호출되는 콜백.
    {
        playerDataNetworkList.Add(new PlayerData // 새 플레이어 데이터를 리스트에 추가.
        {
            clientId = _clientId,
            skinIndex = 0 // 안 씀.
        });
        SetUsernameServerRpc(GetUsername()); // 사용자 이름 설정을 위한 ServerRpc 호출.
    }
    public void StartClient() // 클라이언트 시작.
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += Network_OnClientDisconnectCallback; // 클라이언트 연결 해제 콜백 등록.
        NetworkManager.Singleton.OnClientConnectedCallback += Network_Client_OnClientConnectedCallback; // 클라이언트 연결 콜백 등록.

        NetworkManager.Singleton.StartClient(); // 클라이언트 시작.
    }

    private void Network_Client_OnClientConnectedCallback(ulong obj) // 클라이언트가 연결되었을 때 호출되는 콜백.
    {
        SetUsernameServerRpc(GetUsername()); // 사용자 이름 설정을 위한 ServerRpc 호출.
        if (SceneManager.GetActiveScene().name == "MULTIPLAYER_GAME_SCENE")
        {
            //Scoreboard.Instance.ResetScoreboard();
        }
    }

    //  // 사용자 이름 설정을 위한 ServerRpc.
    [ServerRpc(RequireOwnership = false)]
    void SetUsernameServerRpc(string _username, ServerRpcParams rpcParams = default)
    {
        int playerIndex = GetPlayerDataIndexFromClientID(rpcParams.Receive.SenderClientId); // 클라이언트 ID를 찾아 playerIndex에 넣고.
        PlayerData data = playerDataNetworkList[playerIndex]; // 반환된 값을 playerDataNetworkList에서 찾아 data에 할당.
        data.username = _username; // 사용자 이름 설정.
        playerDataNetworkList[playerIndex] = data; // 바뀐 data를 다시 playerDataNetworkList에 할당.
    }

    private void Network_OnClientDisconnectCallback(ulong clientId) // 클라이언트가 연결을 끊었을 때 호출되는 콜백.
    {
        //Debug.Log("2");
        if (SceneManager.GetSceneByName("MULTIPLAYER_LOBBY_BROWSE_SCENE") == SceneManager.GetActiveScene())
        {
            // failed to connect
            FindObjectOfType<LobbyBrowseUI>().ConnectionFailed();
        }
        else if (SceneManager.GetSceneByName("MULTIPLAYER_LOBBY_JOINED_SCENE") == SceneManager.GetActiveScene())
        {
            // inside a lobby;
            FindObjectOfType<LobbyJoinedUI>().LeaveLobbyPressed();
        }
        else
        {
            // ingame
            //UI.instance.EnableHostDisconnectTab();
        }

        //throw new System.NotImplementedException();
    }

    // 연결 승인 콜백.
    void Network_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        //Debug.Log("1");
        if (gameState.Value != GameState.WaitingToStart)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started.";
            return;
        }
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYERS)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full.";
            return;
        }
        connectionApprovalResponse.Approved = true; // 위 조건 전부 피할 시 연결 승인.
        //connectionApprovalResponse.CreatePlayerObject = true; 
    }

    void LoadLobbyJoinedScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("MULTIPLAYER_LOBBY_JOINED_SCENE", LoadSceneMode.Single);
    }

    public void LoadGameScene()
    {
        LobbyManager.instance.DeleteLobby();

        //string map = PlayerPrefs.GetString("ZOMBIES_MAP", "LAB");
        NetworkManager.Singleton.SceneManager.LoadScene("MULTIPLAYER_GAME_SCENE", LoadSceneMode.Single);
    }

    // 서버에서 연결된 클라이언트에 대해 플레이어 오브젝트를 생성하고 스폰하는 메서드.
    public void SpawnPlayers() // server
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) // NetworkManager.Singleton.ConnectedClientsIds의 clientId 순회.
        {
            GameObject player = Instantiate(playerPrefab); // player 오브젝트에 플레이어 프리팹을 인스턴스화하고.
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true); // 연결된 각 클라이언트 ID에 대해 인스턴스한 프리팹을 NetworkObject를 사용하여 스폰.
        }
    }

}
