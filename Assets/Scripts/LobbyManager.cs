using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance; // instance라는 정적 변수로 LobbyManager 인스턴스 선언.
    const string KEY_RELAY_JOIN_CODE = "RelayJoinCode"; // 상수 문자열 선언.

    Lobby joinedLobby;

    float heartBeatTimer;
    float heartBeatTimerMax = 15;

    float listLobbiesTimer;
    float listLobbiesTimerMax = 3;

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged; // OnLobbyListChangedEventArgs라는 내부 클래스에 OnLobbyListChanged 이벤트 핸들러 선언.
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList; // List<Lobby> 타입 변수 lobbyList.
    }
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject); // 현재 게임오브젝트 객체가 로드될 때 파괴되지 않도록 설정.
        heartBeatTimer = heartBeatTimerMax;
        listLobbiesTimer = listLobbiesTimerMax;

        InitializeUnityAuthentication();
    }

    async void InitializeUnityAuthentication() // '비동기' 메서드 정의.
    {
        if (UnityServices.State != ServicesInitializationState.Initialized) // 유니티 서비스가 초기화되지 않았으면.
        {
            InitializationOptions options = new InitializationOptions(); // InitializationOptions 객체 생성.
            options.SetProfile(UnityEngine.Random.Range(100, 1000).ToString()); // 임의 프로필 생성.
            await UnityServices.InitializeAsync(options); // 해당 메서드 호출하여 서비스 초기화.
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // 해당 메서드 호출하여 익명으로 로그인.
        }
    }
    async Task<Allocation> AllocateRelay() // '비동기' 메서드 정의.
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(NetworkGameManager.MAX_PLAYERS - 1); // RelayService.Instance.CreateAllocationAsync 메서드 호출하여 할당 생성 후.
            return allocation; // 반환.
        }
        catch (RelayServiceException ex) // 예외 시.
        {
            Debug.Log(ex); // 내용 출력하고.
            return default; // 기본값 반환.
        }
    }
    async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId); // RelayService.Instance.GetJoinCodeAsync 메서드 호출 후.
            return relayCode; // 리턴된 조인 코드 반환.
        }
        catch (RelayServiceException ex) // 예외 처리.
        {
            Debug.Log(ex);
            return default;

        }
    }
    async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode); // joinCode 사용하여 RelayService.Instance.JoinAllocationAsync 호출 후.
            return joinAllocation; // 리턴된 할당 객체 반환.
        }
        catch (RelayServiceException ex)
        {
            Debug.Log(ex);
            LobbyBrowseUI.instance.LobbyConnectError(ex.Reason.ToString()); // 이유를 문자열로 반환하여 오류 메시지 표시.
            return default;

        }
    }
    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, NetworkGameManager.MAX_PLAYERS, new CreateLobbyOptions // LobbyService.Instance.CreateLobbyAsync 메서드 호출하여 로비 생성 후 joinedLobby 변수에 할당.
            {
                IsPrivate = isPrivate
            });

            Allocation allocation = await AllocateRelay(); // AllocateRelay() 함수 호출하여 allocation 객체에 할당.
            string relayJoinCode = await GetRelayJoinCode(allocation); // allocation을 받는 GetRelayJoinCode 함수 호출하여 리턴된 조인 코드를 relayJoinCode 변수에 저장.

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions // LobbyService.Instance.UpdateLobbyAsync 함수 호출하여 로비 정보 업데이트.
            {
                Data = new Dictionary<string, DataObject> 
                {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) } // 조인 코드를 로비 데이터에 추가.
                }
            });
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls")); // NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData 호출하여 릴레이 서버 데이터 설정.

            NetworkGameManager.instance.StartHost(); // 호스트 시작.
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
            LobbyBrowseUI.instance.LobbyConnectError(ex.Reason.ToString());
        }
    }
    public async void QuickJoin()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(); // LobbyService.Instance.QuickJoinLobbyAsync() 호출하여 joinedLobby 변수에 할당.

            string relayCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value; // 로비 데이터에서 조인 코드를 가져와 relayCode 변수에 저장.
            JoinAllocation joinAllocation = await JoinRelay(relayCode); // JoinRelay 호출하여 relayCode를 joinAllocation에 저장.

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls")); // 해당 메서드 호출하여 릴레이 서버 데이터 설정.

            NetworkGameManager.instance.StartClient(); // 클라이언트 시작.
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
            LobbyBrowseUI.instance.LobbyConnectError(ex.Reason.ToString());
        }
    }
    public async void JoinByCode(string lobbyCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(lobbyCode))
            {
                Debug.LogError("Lobby code cannot be empty or contain white space.");
                LobbyBrowseUI.instance.LobbyConnectError("Lobby code cannot be empty");
                return;
            }

            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode); // lobbyCode 받는 LobbyService.Instance.JoinLobbyByCodeAsync 메서드 호출하여 joinedLobby 변수에 할당.

            string relayCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value; // 로비 데이터에서 조인 코드 가져와 relayCode에 저장.
            JoinAllocation joinAllocation = await JoinRelay(relayCode); // JoinRelay 메서드 호출하여 리턴된 할당 객체 저장. 

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls")); // 해당 메소드 호출하여 랄레이 서버 데이터 설정.

            NetworkGameManager.instance.StartClient(); // 클라이언트 시작.
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
            LobbyBrowseUI.instance.LobbyConnectError(ex.Reason.ToString());
        }
    }

    // 안 씀.
    public async void JoinByID(string lobbyID)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID);

            string relayCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

           NetworkGameManager.instance.StartClient();
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
            LobbyBrowseUI.instance.LobbyConnectError(ex.Reason.ToString());
        }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId); // 해당 메서드 호출하여 플레이어 로비에서 제거.
                joinedLobby = null;
            }
            catch (LobbyServiceException ex)
            {
                Debug.Log(ex);
            }
        }
    }
    public async void DeleteLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id); // 해당 메소드 호출하여 로비 삭제.
                joinedLobby = null;
            }
            catch (LobbyServiceException ex)
            {
                Debug.Log(ex);
            }
        }
    }

    async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) // 사용 가능 슬롯이 0보다 큰 로비를 필터링하도록 설정.
                },

                Order = new List<QueryOrder>()
                {
                    new QueryOrder(asc: false, field: QueryOrder.FieldOptions.Created) // 로비 생성 날짜 기준 내림차순으로 정렬하도록 설정.
                }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options); // 해당 메서드 호출하여 로비 목록 쿼리하고, 결과를 queryResponse에 저장.
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs // OnLobbyListChanged 이벤트 발생시킨 후.
            {
                lobbyList = queryResponse.Results // 쿼리 결과를 lobbyList에 인자로 전달.
            });
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }

    }

    private void Update() // 매 프레임 호출.
    {
        LobbyHeartBeat();
        LobbyListUpdate();
    }
    void LobbyHeartBeat()
    {
        if (IsLobbyHost()) // 현재 플레이어가 로비 호스트면
        {
            heartBeatTimer -= Time.deltaTime; // heartBeatTimer를 감소시키고.
            if (heartBeatTimer <= 0) // 0 이하가 되면.
            {
                heartBeatTimer = heartBeatTimerMax; // 초기화 후.
                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id); // 해당 메서드를 호출하여 Heartbeat를 전송.
            }
        }
    }
    void LobbyListUpdate()
    {
        if (joinedLobby == null && AuthenticationService.Instance.IsSignedIn && // 로비에 참가하지 않았고 인증 서비스에 로그인돼 있으며.
            SceneManager.GetSceneByName("MULTIPLAYER_LOBBY_BROWSE_SCENE") == SceneManager.GetActiveScene()) // 현재 MULTIPLAYER_LOBBY_BROWSE_SCENE 씬인지 확인.
        {
            listLobbiesTimer -= Time.deltaTime; // listLobbiesTimer를 감소시키고.
            if (listLobbiesTimer <= 0) // 0 이하가 되면.
            {
                listLobbiesTimer = listLobbiesTimerMax; // 초기화 후.
                ListLobbies(); // 해당 메서드 호출하여 로비 목록 업데이트.
            }
        }
    }
    bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId; // 로비에 참가하지 않았고 호스트 아이디와 플레이어 아이디가 동일한지 bool 값 반환.
    }

    public Lobby GetJoinedLobby()
    {
        return joinedLobby; // 현재 참가한 로비 객체 반환.
    }
}
