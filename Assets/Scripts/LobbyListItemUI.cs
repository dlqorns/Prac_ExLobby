using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListItemUI : MonoBehaviour
{
    public TMP_Text lobbyText; // 로비 이름.
    public TMP_Text playerCount; // 플레이어 수.
    Lobby lobby;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => { // 버튼 클릭 시(방 참여 버튼).
            FindObjectOfType<LobbyBrowseUI>().JoinLobbyById(lobby.Id); // lobby.Id는 유니티 lobby 서비스에서 제공하는 각 로비에 부여하는 고유 번호.
        });
    }
    public void SetLobby(Lobby _lobby) // LobbyBrowseUI에서 UpdateLobbyList에 쓰임.
    {
        lobby = _lobby;
        lobbyText.text = lobby.Name; // LobbyText(LobbyListElement 프리팹 캔버스에 있는 로비 이름)에 받은 이름 화면에 나타냄.
        playerCount.text = "" + lobby.Players.Count.ToString() + "/12"; // 플레이어 수 출력.
    }
}
