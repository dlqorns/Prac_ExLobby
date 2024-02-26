using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanup : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if (NetworkManager.Singleton != null)
            Destroy(NetworkManager.Singleton.gameObject);
        if (NetworkGameManager.instance != null)
            Destroy(NetworkGameManager.instance.gameObject);
        if (LobbyManager.instance != null)
            Destroy(LobbyManager.instance.gameObject);
    }
}