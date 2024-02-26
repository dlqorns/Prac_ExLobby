using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ReadyUp : NetworkBehaviour
{
    LobbyJoinedUI ui;
    Dictionary<ulong, bool> playerReadyDictionary;

    public bool isLocalPlayerReady = false;
    // Start is called before the first frame update
    private void Awake()
    {
        playerReadyDictionary = new Dictionary<ulong, bool>();
        ui = GetComponent<LobbyJoinedUI>();
    }
    public void SetLocalPlayerReady()
    {
        isLocalPlayerReady = true;
        SetPlayerReadyServerRpc(NetworkGameManager.instance.GetPlayerDataIndexFromClientID(NetworkManager.Singleton.LocalClientId));
        //Debug.Log(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerReadyServerRpc(int _index, ServerRpcParams rpcParams = default) // server
    {
        playerReadyDictionary[rpcParams.Receive.SenderClientId] = true;

        SetPlayerReadyDisplayClientRpc(_index);

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                //this player not ready
                allClientsReady = false;
                break;
            }
        }
        Debug.Log("All clients ready: " + allClientsReady);
        if (allClientsReady == true)
        {
            NetworkGameManager.instance.LoadGameScene();
        }
    }

    [ClientRpc]
    void SetPlayerReadyDisplayClientRpc(int _index)
    {
        //GameManager.instance.GetPlayerDataFromIndex(_index);
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

    public void SetLocalPlayerUnready()
    {
        isLocalPlayerReady = false;
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
