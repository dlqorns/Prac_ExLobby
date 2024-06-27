using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanup : MonoBehaviour
{
    // 각 필드가 비어있지 않으면 gameObject Destroy함.
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