using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    //SkinSelector skinSelector;
    public int index; // 로비에 표시되는 플레이어들의 인덱스값.

    public TMP_Text usernameText;

    private void Start()
    {
        NetworkGameManager.onPlayerDataListChanged += UpdatePlayer; // PlayerDataList가 바뀌면 UpdatePlayer 이벤트 핸들러 호출.
        //skinSelector = GetComponent<SkinSelector>();
        UpdatePlayer(); // UpdatePlayer 함수 호출.
    }

    void UpdatePlayer()
    {
        //Debug.Log("Update");
        if (NetworkGameManager.instance.IsPlayerIndexConnected(index))
        {
            Show();

            PlayerData data = NetworkGameManager.instance.GetPlayerDataFromIndex(index); // GetPlayerDataFromIndex(index) 값을 PlayerData 객체인 data에 넣음.
            //skinSelector.UpdateSkin(NetworkGameManager.instance.GetPlayerSkinFromIndex(index));
            usernameText.text = data.username.ToString(); // data.username를 문자열로 바꾸어(ToString()) usernameText을 화면에 표시함(.text).
        }
        else
        {
            Hide(); 
        }
    }

    void Show()
    {
        //FindObjectOfType<CharacterSelectUI>().skinSelector = skinSelector;
        gameObject.SetActive(true); // gameObject를 활성화함.
        //skinSelector.ChangeSkinIndex(GameManager.instance.GetPlayerSkin(index));

    }
    void Hide()
    {
        gameObject.SetActive(false); // gameObject를 비활성화함.
    }

    public override void OnDestroy() // OnDestroy(): 게임 오브젝트 파괴 시 호출. 
    {
        NetworkGameManager.onPlayerDataListChanged -= UpdatePlayer; // UpdatePlayer 이벤트 핸들러 제거.
    }
}
