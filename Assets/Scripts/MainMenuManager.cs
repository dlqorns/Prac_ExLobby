using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
   public void LoadLobbyBrowseScene()
    {
        SceneManager.LoadScene("MULTIPLAYER_LOBBY_BROWSE_SCENE");
    }
}
