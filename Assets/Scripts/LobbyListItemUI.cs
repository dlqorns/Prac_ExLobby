using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListItemUI : MonoBehaviour
{
    public TMP_Text lobbyText;
    public TMP_Text playerCount;
    Lobby lobby;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => {
            FindObjectOfType<LobbyBrowseUI>().JoinLobbyById(lobby.Id);
        });
    }
    public void SetLobby(Lobby _lobby)
    {
        lobby = _lobby;
        lobbyText.text = lobby.Name;
        playerCount.text = "" + lobby.Players.Count.ToString() + "/12";
    }
}
