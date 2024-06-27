using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
   public void LoadLobbyBrowseScene() // PlayOnline 버튼 누르면 호출되는 함수
    {
        SceneManager.LoadScene("MULTIPLAYER_LOBBY_BROWSE_SCENE"); // MULTIPLAYER_LOBBY_BROWSE_SCENE 씬 불러옴
    }
}
