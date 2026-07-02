using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Collections.Generic;

namespace MoonsAndStars.Assets.Code.Scripts.Multiplayer
{
    public class UnityGameServicesManager : MonoBehaviour
    {
        public static UnityGameServicesManager Instance { get; private set; }
        
        [Header("Settings")]
        [SerializeField] private int _maxPlayers = 4;
        
        private Lobby _currentLobby;
        private string _joinCode;
        private float _heartbeatTimer;
        private float _lobbyUpdateTimer;
        
        public string LocalPlayerName { get; private set; } = "Player";
        public int LocalHostNpcCountConfig { get; private set; } = 0;

        public event Action OnSignedIn;
        public event Action OnSignedOut;
        public event Action<Lobby> OnLobbyUpdated;
        public event Action<string> OnJoinCodeReceived;
        public event Action OnLobbyJoined;
        public event Action OnLobbyLeft;
        public event Action OnSceneLoadComplete;
        
        public bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;
        public Lobby CurrentLobby => _currentLobby;
        public string JoinCode => _joinCode;
        
        private async void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                await InitializeServices();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted += SubscribeToSceneManager;
                NetworkManager.Singleton.OnClientStarted += SubscribeToSceneManager;
            }
        }

        private void SubscribeToSceneManager()
        {
            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
            }
        }

        private void HandleSceneEvent(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
            {
                Debug.Log($"[UGS DEBUG] Scene loaded successfully: {sceneEvent.SceneName}");
                OnSceneLoadComplete?.Invoke();
            }
        }
        
        private void Update()
        {
            if (_currentLobby != null)
            {
                _heartbeatTimer += Time.deltaTime;
                if (_heartbeatTimer > 15f)
                {
                    _heartbeatTimer = 0;
                    _ = HeartbeatLobbyAsync();
                }
                
                _lobbyUpdateTimer += Time.deltaTime;
                if (_lobbyUpdateTimer > 3f)
                {
                    _lobbyUpdateTimer = 0;
                    _ = UpdateLobbyDataAsync();
                }
            }
        }
        
        private async Task InitializeServices()
        {
            try
            {
                InitializationOptions options = new InitializationOptions();
                #if UNITY_EDITOR
                    options.SetProfile("UnityEditor_User");
                #else
                    options.SetProfile($"LinuxBuild_{UnityEngine.Random.Range(1000, 9999)}");
                #endif
                options.SetOption("com.unity.services.core.environment-name", "production");

                await UnityServices.InitializeAsync(options);
                await SignInAnonymously();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UGS CRITICAL ERROR] Failed to initialize services: {e.Message}");
            }
        }
        
        private async Task SignInAnonymously()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                OnSignedIn?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UGS AUTH ERROR] Failed to sign in anonymously: {e.Message}");
            }
        }

        public void SetLocalPlayerName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name)) LocalPlayerName = name;
        }
        
        public async Task<string> CreateLobby(string playerName, bool isPrivate = false)
        {
            try
            {
                if (string.IsNullOrEmpty(playerName)) playerName = $"Player_{UnityEngine.Random.Range(100,999)}";

                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(_maxPlayers);
                string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                
                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Player = GetPlayer(playerName),
                    Data = new Dictionary<string, DataObject>
                    {
                        { "Started", new DataObject(DataObject.VisibilityOptions.Public, "False") },
                        { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) },
                        { "NpcCount", new DataObject(DataObject.VisibilityOptions.Public, "0") }
                    }
                };
                
                _currentLobby = await LobbyService.Instance.CreateLobbyAsync("GameLobby", _maxPlayers, options);
                _joinCode = _currentLobby.LobbyCode;
                
                var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
                utp.SetRelayServerData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData,
                    allocation.ConnectionData 
                );
                
                NetworkManager.Singleton.StartHost();
                OnJoinCodeReceived?.Invoke(_joinCode);
                OnLobbyJoined?.Invoke();
                
                return _joinCode;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LOBBY CRITICAL ERROR] Failed to host game via UGS: {e.Message}");
                return null;
            }
        }
        
        public async Task JoinLobby(string lobbyCode, string playerName)
        {
            try
            {
                if (string.IsNullOrEmpty(playerName)) playerName = $"Player_{UnityEngine.Random.Range(100, 999)}";
                
                JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions { Player = GetPlayer(playerName) };
                _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
                _joinCode = lobbyCode;
                
                if (_currentLobby.Data != null && _currentLobby.Data.ContainsKey("RelayJoinCode"))
                {
                    string actualRelayCode = _currentLobby.Data["RelayJoinCode"].Value;
                    JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(actualRelayCode);
                    
                    var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
                    utp.SetRelayServerData(
                        allocation.RelayServer.IpV4,
                        (ushort)allocation.RelayServer.Port,
                        allocation.AllocationIdBytes,
                        allocation.Key,
                        allocation.ConnectionData,
                        allocation.HostConnectionData
                    );
                    
                    NetworkManager.Singleton.StartClient();
                    OnLobbyJoined?.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LOBBY JOIN ERROR] Failed to connect to lobby with code {lobbyCode}: {e.Message}");
            }
        }
        
        public async Task LeaveLobby()
        {
            if (_currentLobby != null)
            {
                try { await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, AuthenticationService.Instance.PlayerId); }
                catch {}
                
                _currentLobby = null;
                _joinCode = null;
                if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
                OnLobbyLeft?.Invoke();
            }
        }
        
        public async Task StartGame(int npcCount)
        {
            if (_currentLobby != null && NetworkManager.Singleton.IsHost)
            {
                LocalHostNpcCountConfig = npcCount;

                try
                {
                    await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions
                    {
                        Data = new Dictionary<string, DataObject>
                        {
                            { "Started", new DataObject(DataObject.VisibilityOptions.Public, "True") },
                            { "NpcCount", new DataObject(DataObject.VisibilityOptions.Public, npcCount.ToString()) }
                        }
                    });
                    
                    await Task.Delay(1000);

                    NetworkManager.Singleton.SceneManager.LoadScene("Space", UnityEngine.SceneManagement.LoadSceneMode.Single);
                }
                catch (Exception e) { Debug.LogError($"Failed to start game: {e.Message}"); }
            }
        }
        
        private async Task HeartbeatLobbyAsync()
        {
            if (_currentLobby != null && NetworkManager.Singleton.IsHost)
            {
                try { await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id); }
                catch {}
            }
        }
        
        private async Task UpdateLobbyDataAsync()
        {
            if (_currentLobby != null)
            {
                try
                {
                    _currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);
                    OnLobbyUpdated?.Invoke(_currentLobby);
                }
                catch {}
            }
        }
        
        private Player GetPlayer(string playerName)
        {
            return new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                }
            };
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.OnServerStarted -= SubscribeToSceneManager;
                NetworkManager.Singleton.OnClientStarted -= SubscribeToSceneManager;
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
            }
        }
    }
}