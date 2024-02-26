using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager instance;
    public const int MAX_PLAYERS = 12;
    public GameObject playerPrefab;

    public NetworkList<PlayerData> playerDataNetworkList;
    public delegate void OnPlayerDataListChanged();
    public static OnPlayerDataListChanged onPlayerDataListChanged;

    public GameObject myPlayer; // only set when ingame;

    string username;
    public enum GameState // only allow players to join while waiting to start
    {
        WaitingToStart,
        InHub,
        InGame,
        End
    }
    public NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.WaitingToStart);
    private void Awake()
    {
        if (NetworkGameManager.instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        playerDataNetworkList = new NetworkList<PlayerData>();
        DontDestroyOnLoad(gameObject);

        username = PlayerPrefs.GetString("USERNAME", "Guest: " + Random.Range(100, 1000));
    }

    private void Start()
    {
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    public string GetUsername()
    {
        return username;
    }
    public void SetUsername(string _username)
    {
        if (string.IsNullOrWhiteSpace(_username))
        {
            username = "Guest: " + Random.Range(100, 1000);
        }
        else
        {
             username = _username;
        }

        PlayerPrefs.SetString("USERNAME", username);
    }

    public string GetUsernameFromClientId(ulong _clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == _clientId)
                return playerData.username.ToString();
        }
        return default;
    }
    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        //Debug.Log("Invoke");
        //Debug.Log(playerDataNetworkList.Count);
        onPlayerDataListChanged?.Invoke();
    }
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }
    public PlayerData GetPlayerDataFromIndex(int _playerIndex)
    {
        return playerDataNetworkList[_playerIndex];
    }
    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
                return playerData;
        }
        return default;
    }
    public PlayerData GetLocalPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }
    public int GetPlayerDataIndexFromClientID(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
                return i;
        }
        return -1;
    }
    public int GetPlayerSkinFromIndex(int _playerIndex)
    {
        return playerDataNetworkList[_playerIndex].skinIndex;
    }

    public void ChangePlayerSkin(int skinIndex)
    {
        ChangePlayerSkinServerRpc(skinIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    void ChangePlayerSkinServerRpc(int skinIndex, ServerRpcParams rpcParams = default)
    {
        int playerIndex = GetPlayerDataIndexFromClientID(rpcParams.Receive.SenderClientId);
        PlayerData data = playerDataNetworkList[playerIndex];
        data.skinIndex = skinIndex;
        playerDataNetworkList[playerIndex] = data;
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += Network_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += Network_Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Network_Server_OnClientDisconnectCallback;

        NetworkManager.Singleton.StartHost();
        LoadLobbyJoinedScene();
    }

    private void Network_Server_OnClientDisconnectCallback(ulong _clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData data = playerDataNetworkList[i];
            if (data.clientId == _clientId)
            {
                playerDataNetworkList.RemoveAt(i);
            }
        }

        if (SceneManager.GetActiveScene().name == "MULTIPLAYER_GAME_SCENE")
        {
            //Scoreboard.Instance.ResetScoreboard();
        }
    }
    private void Network_Server_OnClientConnectedCallback(ulong _clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = _clientId,
            skinIndex = 0
        });
        SetUsernameServerRpc(GetUsername());
    }
    public void StartClient()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += Network_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += Network_Client_OnClientConnectedCallback;

        NetworkManager.Singleton.StartClient();
    }

    private void Network_Client_OnClientConnectedCallback(ulong obj)
    {
        SetUsernameServerRpc(GetUsername());
        if (SceneManager.GetActiveScene().name == "MULTIPLAYER_GAME_SCENE")
        {
            //Scoreboard.Instance.ResetScoreboard();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    void SetUsernameServerRpc(string _username, ServerRpcParams rpcParams = default)
    {
        int playerIndex = GetPlayerDataIndexFromClientID(rpcParams.Receive.SenderClientId);
        PlayerData data = playerDataNetworkList[playerIndex];
        data.username = _username;
        playerDataNetworkList[playerIndex] = data;
    }

    private void Network_OnClientDisconnectCallback(ulong clientId)
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
        connectionApprovalResponse.Approved = true;
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

    public void SpawnPlayers() // server
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject player = Instantiate(playerPrefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

}
