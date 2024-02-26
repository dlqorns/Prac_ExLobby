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

    public TMP_InputField lobbyNameInput;
    public Toggle isPrivate;

    public TMP_InputField joinCodeInput;
    public TMP_InputField usernameInput;

    public GameObject lobbyContainer;
    public GameObject lobbyListTemplate;

    public GameObject connectionResponseUI;
    public TMP_Text messsageText;
    public GameObject connectionResponseCloseButton;

    public static LobbyBrowseUI instance;

    private void Awake()
    {
        instance = this;

        lobbyListTemplate.SetActive(false);
        lobbyContainer.SetActive(true);
    }

    private void Start()
    {
        usernameInput.text = NetworkGameManager.instance.GetUsername();
        usernameInput.onValueChanged.AddListener((string newText) =>
        {
            NetworkGameManager.instance.SetUsername(newText);
        });

        LobbyManager.instance.OnLobbyListChanged += GameLobby_OnLobbyListChanged;
    }

    private void GameLobby_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }
    public void CreateLobbyPressed()
    {
        //NetworkManager.Singleton.StartHost();
        mainPanel.SetActive(false);
        connectionResponseUI.SetActive(true);
        string _lobby = "Lobby";
        messsageText.text = "Connecting...";

        if (lobbyNameInput.text == "" || lobbyNameInput.text == null)
        {
            _lobby = ("Lobby " + UnityEngine.Random.Range(100, 1000).ToString());
        }
        else
        {
            _lobby = lobbyNameInput.text;
        }
        LobbyManager.instance.CreateLobby(_lobby, isPrivate.isOn);
        //LoadCharacterSelectScene();
    }
    public void QuickJoinPressed()
    {
        mainPanel.SetActive(false);
        lobbyContainer.SetActive(false);
        connectionResponseUI.SetActive(true);
        messsageText.text = "Connecting...";

        LobbyManager.instance.QuickJoin();
        //NetworkManager.Singleton.StartClient();
    }
    public void JoinCodePressed()
    {
        mainPanel.SetActive(false);
        lobbyContainer.SetActive(false);
        connectionResponseUI.SetActive(true);
        messsageText.text = "Connecting...";

        LobbyManager.instance.JoinByCode(joinCodeInput.text);
    }

    public void JoinLobbyById(string _lobbyId)
    {
        mainPanel.SetActive(false);
        lobbyContainer.SetActive(false);
        connectionResponseUI.SetActive(true);
        messsageText.text = "Connecting...";

        LobbyManager.instance.JoinByID(_lobbyId);
    }

    public void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in lobbyContainer.transform)
        {
            if (child == lobbyListTemplate) continue;
            Destroy(child.gameObject);
        }
        foreach (Lobby lobby in lobbyList)
        {
            GameObject _lobby = Instantiate(lobbyListTemplate, lobbyContainer.transform);
            _lobby.SetActive(true);
            _lobby.GetComponent<LobbyListItemUI>().SetLobby(lobby);
        }
    }

    public void ConnectionFailed()
    {
        messsageText.text = NetworkManager.Singleton.DisconnectReason.ToString();
        connectionResponseCloseButton.SetActive(true);
    }
    public void LobbyConnectError(string reason)
    {
        messsageText.text = reason;
        connectionResponseCloseButton.SetActive(true);
    }
    public void CloseConnectionResponseUI()
    {
        connectionResponseUI.SetActive(false);
        connectionResponseCloseButton.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    private void OnDestroy()
    {
        LobbyManager.instance.OnLobbyListChanged -= GameLobby_OnLobbyListChanged;
    }
}
