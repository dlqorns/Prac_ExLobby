using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    //SkinSelector skinSelector;
    public int index;

    public TMP_Text usernameText;

    private void Start()
    {
        NetworkGameManager.onPlayerDataListChanged += UpdatePlayer;
        //skinSelector = GetComponent<SkinSelector>();
        UpdatePlayer();
    }

    void UpdatePlayer()
    {
        //Debug.Log("Update");
        if (NetworkGameManager.instance.IsPlayerIndexConnected(index))
        {
            Show();

            PlayerData data = NetworkGameManager.instance.GetPlayerDataFromIndex(index);
            //skinSelector.UpdateSkin(NetworkGameManager.instance.GetPlayerSkinFromIndex(index));
            usernameText.text = data.username.ToString();
        }
        else
        {
            Hide();
        }
    }

    void Show()
    {
        //FindObjectOfType<CharacterSelectUI>().skinSelector = skinSelector;
        gameObject.SetActive(true);
        //skinSelector.ChangeSkinIndex(GameManager.instance.GetPlayerSkin(index));

    }
    void Hide()
    {
        gameObject.SetActive(false);
    }

    public override void OnDestroy()
    {
        NetworkGameManager.onPlayerDataListChanged -= UpdatePlayer;
    }
}
