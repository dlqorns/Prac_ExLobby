using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LobbyJoinedUI : MonoBehaviour
{
    public GameObject readyButton;
   // public GameObject[] arrows;

    public GameObject unreadyButton;

    public TMP_Text lobbyNameText;
    public TMP_Text lobbyCodeText;

    public GameObject leaveLobbyButton;

    private void Awake()
    {
        readyButton.SetActive(true);

        //arrows[0].SetActive(true);
        //arrows[1].SetActive(true);
    }

    private void Start()
    {
        Lobby lobby = LobbyManager.instance.GetJoinedLobby();
        lobbyNameText.text = "Lobby: " + lobby.Name;
        lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;

        //NetworkGameManager.instance.ChangePlayerSkin(Random.Range(0, 111));
    }
    public void ReadyPressed()
    {
        readyButton.SetActive(false);
        unreadyButton.SetActive(true);

        leaveLobbyButton.SetActive(false);
    }

    public void UnReadyPressed()
    {
        unreadyButton.SetActive(false);
        readyButton.SetActive(true);

        leaveLobbyButton.SetActive(true);

    }

    public void arrowPressed(int amount)
    {
        int id = NetworkGameManager.instance.GetPlayerDataIndexFromClientID(NetworkManager.Singleton.LocalClientId);
        int index = NetworkGameManager.instance.GetPlayerSkinFromIndex(id);
        index += amount;
        if (index > 111)
            index = 0;
        if (index < 0)
            index = 111;
        NetworkGameManager.instance.ChangePlayerSkin(index);
    }

    public void LeaveLobbyPressed()
    {
        LobbyManager.instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MAINMENU_SCENE");
    }
}
