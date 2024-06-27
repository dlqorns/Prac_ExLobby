using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour // NetworkBehaviour를 상속 받음.
{
    public override void OnNetworkSpawn() // NetworkBehaviour의 OnNetworkSpawn 메서드 재정의
    {
        base.OnNetworkSpawn(); // 기존 OnNetworkSpawn 메서드 호출.
        if (IsHost) // 호스트가 맞으면
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted; // LoadEvent가 완료될 때 호출될 이벤트 핸들러를 등록함.
        } // 이벤트 핸들러: 특정 요소에서 발생하는 이벤트를 처리하기 위한 함수.
    }

    // LoadEvent 완료 시 sceneName, loadSceneMode, clientsCompleted, clientsTimedOut을 매개변수로 받는 SceneManager_OnLoadEventCompleted 메서드 실행.
    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        // server
        NetworkGameManager.instance.SpawnPlayers(); // SpawnPlayers 메서드 호출

        if (IsServer)
        {
            // 
        }
    }
}
