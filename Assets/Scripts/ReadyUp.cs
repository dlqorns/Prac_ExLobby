using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ReadyUp : NetworkBehaviour
{
    LobbyJoinedUI ui;
    Dictionary<ulong, bool> playerReadyDictionary; // ulong과 bool 값 키를 가지는 Dictionary 변수.

    public bool isLocalPlayerReady = false;
    // Start is called before the first frame update
    private void Awake()
    {
        playerReadyDictionary = new Dictionary<ulong, bool>(); // playerReadyDictionary 초기화.
        ui = GetComponent<LobbyJoinedUI>(); // LobbyJoinedUI 컴포넌트 가져와 ui 변수에 할당.
    }
    public void SetLocalPlayerReady() // 로컬 플레이어 레디 함수.
    {
        isLocalPlayerReady = true; // 로컬 플레이어 레디 true로 함. 
        SetPlayerReadyServerRpc(NetworkGameManager.instance.GetPlayerDataIndexFromClientID(NetworkManager.Singleton.LocalClientId)); // SetPlayerReadyServerRpc 호출하여 로컬 클라이언트 아이디를 사용해 플레이어 데이터 가져와 전달. 
        //Debug.Log(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)] 
    // _index, rpcParams를 매개 변수로 받는 SetPlayerReadyServerRpc 함수 정의.
    void SetPlayerReadyServerRpc(int _index, ServerRpcParams rpcParams = default) // server
    {
        playerReadyDictionary[rpcParams.Receive.SenderClientId] = true; // 해당 키를 true로 설정.

        SetPlayerReadyDisplayClientRpc(_index); // _index 값을 받는 SetPlayerReadyDisplayClientRpc 호출.

        bool allClientsReady = true; // 모든 클라이언트가 준비되었는지 논리 작성.
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) // ConnectedClientsIds의 clientId 순회.
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId]) // 준비되지 않은 클라이언트가 있으면.
            {
                //this player not ready
                allClientsReady = false; // false로 값 두고.
                break; // 종료.
            }
        }
        Debug.Log("All clients ready: " + allClientsReady); // 모두 준비됐는지 여부 출력.
        if (allClientsReady == true) // 준비 다 됐으면.
        {
            NetworkGameManager.instance.LoadGameScene(); // 게임 씬 로드함.
        }
    }

    [ClientRpc]
    void SetPlayerReadyDisplayClientRpc(int _index)
    {
        //GameManager.instance.GetPlayerDataFromIndex(_index);
        // LobbyPlayer 객체를 찾아 인덱스가 _index인 플레이어 유저네임 색깔을 초록색으로 바꿈.
        LobbyPlayer[] _players = FindObjectsOfType<LobbyPlayer>();
        foreach (LobbyPlayer player in _players)
        {
            if (player.index == _index)
            {
                player.usernameText.color = Color.green;
                break;
            }
        }
    }

    public void SetLocalPlayerUnready() // 로컬 플레이어 언레디.
    {
        isLocalPlayerReady = false; // false.
        SetPlayerUnreadyServerRpc(NetworkGameManager.instance.GetPlayerDataIndexFromClientID(NetworkManager.Singleton.LocalClientId));
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerUnreadyServerRpc(int _index, ServerRpcParams rpcParams = default)
    {
        playerReadyDictionary[rpcParams.Receive.SenderClientId] = false;
        SetPlayerUnreadyDisplayClientRpc(_index);
    }
    [ClientRpc]
    void SetPlayerUnreadyDisplayClientRpc(int _index)
    {
        // 준비 안 된 플레이어 유저네임 색상을 하얀색으로 바꿈.
        LobbyPlayer[] _players = FindObjectsOfType<LobbyPlayer>();
        foreach (LobbyPlayer player in _players)
        {
            if (player.index == _index)
            {
                player.usernameText.color = Color.white;
                break;
            }
        }
    }
}
