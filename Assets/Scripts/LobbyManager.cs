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
    public static LobbyManager instance;
    const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";

    Lobby joinedLobby;

    float heartBeatTimer;
    float heartBeatTimerMax = 15;

    float listLobbiesTimer;
    float listLobbiesTimerMax = 3;

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        heartBeatTimer = heartBeatTimerMax;
        listLobbiesTimer = listLobbiesTimerMax;

        InitializeUnityAuthentication();
    }

    async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions options = new InitializationOptions();
            options.SetProfile(UnityEngine.Random.Range(100, 1000).ToString());
            await UnityServices.InitializeAsync(options);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
    async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(NetworkGameManager.MAX_PLAYERS - 1);
            return allocation;
        }
        catch (RelayServiceException ex)
        {
            Debug.Log(ex);
            return default;
        }
    }
    async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayCode;
        }
        catch (RelayServiceException ex)
        {
            Debug.Log(ex);
            return default;

        }
    }
    async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;
        }
        catch (RelayServiceException ex)
        {
            Debug.Log(ex);
            LobbyBrowseUI.instance.LobbyConnectError(ex.Reason.ToString());
            return default;

        }
    }
    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, NetworkGameManager.MAX_PLAYERS, new CreateLobbyOptions
            {
                IsPrivate = isPrivate
            });

            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            NetworkGameManager.instance.StartHost();
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
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

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

            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

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
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
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
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
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
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },

                Order = new List<QueryOrder>()
                {
                    new QueryOrder(asc: false, field: QueryOrder.FieldOptions.Created)
                }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs
            {
                lobbyList = queryResponse.Results
            });
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }

    }

    private void Update()
    {
        LobbyHeartBeat();
        LobbyListUpdate();
    }
    void LobbyHeartBeat()
    {
        if (IsLobbyHost())
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer <= 0)
            {
                heartBeatTimer = heartBeatTimerMax;
                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }
    void LobbyListUpdate()
    {
        if (joinedLobby == null && AuthenticationService.Instance.IsSignedIn &&
            SceneManager.GetSceneByName("MULTIPLAYER_LOBBY_BROWSE_SCENE") == SceneManager.GetActiveScene())
        {
            listLobbiesTimer -= Time.deltaTime;
            if (listLobbiesTimer <= 0)
            {
                listLobbiesTimer = listLobbiesTimerMax;
                ListLobbies();
            }
        }
    }
    bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }
}
