using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyBrowseUI : MonoBehaviour
{
    public GameObject mainPanel;

    public TMP_InputField lobbyNameInput; // 로비 이름 입력란.
    public Toggle isPrivate; // 비밀방으로 할 것인지.

    public TMP_InputField joinCodeInput; // 조인 코드 입력란.
    public TMP_InputField usernameInput; // 유저네임 입력란.

    public GameObject lobbyContainer; // 로비 방들이 나열될 뷰(Container).
    public GameObject lobbyListTemplate; // 나열될 로비 방 오브젝트 프리팹 넣는 칸.

    public GameObject connectionResponseUI; // 연결 중 또는 연결 실패 시 뜰 UI.
    public TMP_Text messsageText; // connectionResponseUI에 뜰 Text.
    public GameObject connectionResponseCloseButton; // connectionResponseUI에 뜰 Back 버튼.

    public static LobbyBrowseUI instance; // LobbyBrowseUI 클래스의 인스턴스를 저장하는 정적 변수. 다른 클래스에서 LobbyBrowseUI의 메서드, 변수에 접근할 수 있어짐.

    private void Awake()
    {
        instance = this;

        lobbyListTemplate.SetActive(false); // 프리팹 비활성화.
        lobbyContainer.SetActive(true); // Container는 활성화.
    }

    private void Start()
    {
        usernameInput.text = NetworkGameManager.instance.GetUsername(); // 유저네임 입력 받음.
        usernameInput.onValueChanged.AddListener((string newText) => // 받아서 newText 변수에 저장하고.
        {
            NetworkGameManager.instance.SetUsername(newText); // SetUsername 함수의 매개 변수에 newText 넣어줌.
        });

        LobbyManager.instance.OnLobbyListChanged += GameLobby_OnLobbyListChanged; // 로비 리스트가 바뀌면 GameLobby_OnLobbyListChanged 이벤트 핸들러 등록.
    }

    private void GameLobby_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList); // 로비 리스트 업데이트 함수의 매개 변수에 LobbyManager.OnLobbyListChangedEventArgs e 넣어줌.
    }
    public void CreateLobbyPressed() // CreateLobby 누르면.
    {
        //NetworkManager.Singleton.StartHost();
        mainPanel.SetActive(false); // mainPanel 비활성화.
        connectionResponseUI.SetActive(true); // connectionResponseUI 활성화.
        string _lobby = "Lobby";
        messsageText.text = "Connecting..."; // connectionResponseUI에 해당 메시지 띄움.

        if (lobbyNameInput.text == "" || lobbyNameInput.text == null) // 로비 이름 입력 안 하면.
        {
            _lobby = ("Lobby " + UnityEngine.Random.Range(100, 1000).ToString()); // 랜덤으로 로비 이름 설정.
        }
        else
        {
            _lobby = lobbyNameInput.text; // 아닐 땐 작성한대로 로비 이름 설정.
        }
        LobbyManager.instance.CreateLobby(_lobby, isPrivate.isOn); // 로비 이름과 isPrivate 상태 LobbyManager의 CreateLobby 함수에 매개 변수로 담아 호출.
        //LoadCharacterSelectScene();
    }
    public void QuickJoinPressed() // 퀵 조인 누르면.
    {
        mainPanel.SetActive(false);
        lobbyContainer.SetActive(false);
        connectionResponseUI.SetActive(true);
        messsageText.text = "Connecting...";

        LobbyManager.instance.QuickJoin(); // LobbyManager의 QuickJoin() 호출.
        //NetworkManager.Singleton.StartClient();
    }
    public void JoinCodePressed() // 조인 코드 누르면
    {
        mainPanel.SetActive(false);
        lobbyContainer.SetActive(false);
        connectionResponseUI.SetActive(true);
        messsageText.text = "Connecting...";

        LobbyManager.instance.JoinByCode(joinCodeInput.text); // 조인코드 입력을 매개 변수로 받는 LobbyManager의 QuickJoin() 호출.
    }

    // 이 로비 아이디는 유니티 측해서 제공하는 로비 아이디 관련 함수.
    public void JoinLobbyById(string _lobbyId)
    {
        mainPanel.SetActive(false);
        lobbyContainer.SetActive(false);
        connectionResponseUI.SetActive(true);
        messsageText.text = "Connecting...";

        LobbyManager.instance.JoinByID(_lobbyId);
    }

    public void UpdateLobbyList(List<Lobby> lobbyList) // 로비 리스트 업데이트 함수.
    {
        foreach (Transform child in lobbyContainer.transform) // Container에 맞게 child 배치.
        {
            if (child == lobbyListTemplate) continue; // child가 lobbyListTemplate에 들어가는 프리팹이 맞으면 반복.
            Destroy(child.gameObject); // 아니면 해당 오브젝트 파괴.
        }
        foreach (Lobby lobby in lobbyList) // 로비 리스트의 로비 순회.
        {
            GameObject _lobby = Instantiate(lobbyListTemplate, lobbyContainer.transform);
            _lobby.SetActive(true);
            _lobby.GetComponent<LobbyListItemUI>().SetLobby(lobby);
        }
    }

    public void ConnectionFailed() // 연결 실패 시.
    {
        messsageText.text = NetworkManager.Singleton.DisconnectReason.ToString(); // 이유를 문자열로 바꾸어 connectionResponseUI의 텍스트에 띄움.
        connectionResponseCloseButton.SetActive(true); // connectionResponseUI의 Back 버튼 활성화.
    }
    public void LobbyConnectError(string reason) // 로비 연결 에러 시.
    {
        messsageText.text = reason; // 이유를 connectionResponseUI의 텍스트에 띄움.
        connectionResponseCloseButton.SetActive(true); // connectionResponseUI의 Back 버튼 활성화.
    }
    public void CloseConnectionResponseUI() // connectionResponseUI를 닫는 함수.
    {
        connectionResponseUI.SetActive(false); // connectionResponseUI 비활성화.
        connectionResponseCloseButton.SetActive(false); // connectionResponseUI의 Back 버튼 비활성화.
        mainPanel.SetActive(true); // mainPanel 활성화.
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0); // 0번 씬으로 이동.
    }

    private void OnDestroy()
    {
        LobbyManager.instance.OnLobbyListChanged -= GameLobby_OnLobbyListChanged; // 이벤트 핸들러 등록 해제.
    }
}
