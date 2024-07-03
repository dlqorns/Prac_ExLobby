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
        readyButton.SetActive(true); // readyButton 활성화.

        //arrows[0].SetActive(true);
        //arrows[1].SetActive(true);
    }

    private void Start()
    {
        Lobby lobby = LobbyManager.instance.GetJoinedLobby(); // GetJoindeLobby 불러와 Lobby 객체인 lobby에 대입.
        lobbyNameText.text = "Lobby: " + lobby.Name; // 로비 이름이 화면에 보여짐.
        lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode; // 로비 코드가 화면에 보여짐.

        //NetworkGameManager.instance.ChangePlayerSkin(Random.Range(0, 111));
    }
    public void ReadyPressed() // 레디 누르면.
    {
        readyButton.SetActive(false); // 레디 버튼은 비활성화.
        unreadyButton.SetActive(true); // 언레디 버튼은 활성화.

        leaveLobbyButton.SetActive(false); // 로비 떠나기 버튼 비활성화.
    }

    public void UnReadyPressed() // 언레디 누르면.
    {
        unreadyButton.SetActive(false); // 언레디 버튼은 비활성화.
        readyButton.SetActive(true); // 레디 버튼은 활성화.

        leaveLobbyButton.SetActive(true); // 로비 떠나기 버튼 활성화.

    }

    // 스킨 안 쓸 거니 필요 없는 함수.
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

    public void LeaveLobbyPressed() // 로비 떠나기 누르면.
    {
        LobbyManager.instance.LeaveLobby(); // LeaveLobby() 호출.
        NetworkManager.Singleton.Shutdown(); // Shutdown() 호출.
        SceneManager.LoadScene("MAINMENU_SCENE"); // "MAINMENU_SCENE" 씬 불러옴.
    }
}
