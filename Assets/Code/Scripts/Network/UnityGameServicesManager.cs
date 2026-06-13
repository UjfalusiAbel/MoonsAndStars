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
        
        public event Action OnSignedIn;
        public event Action OnSignedOut;
        public event Action<Lobby> OnLobbyUpdated;
        public event Action<string> OnJoinCodeReceived;
        public event Action OnLobbyJoined;
        public event Action OnLobbyLeft;
        
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
                await UnityServices.InitializeAsync();
                await SignInAnonymously();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize services: {e.Message}");
            }
        }
        
        private async Task SignInAnonymously()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Signed in as: {AuthenticationService.Instance.PlayerId}");
                OnSignedIn?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to sign in: {e.Message}");
            }
        }
        
        public async Task<string> CreateLobby(bool isPrivate = false)
        {
            try
            {
                string playerId = AuthenticationService.Instance.PlayerId;
                string shortId = playerId.Length > 6 ? playerId.Substring(0, 6) : playerId;
                string playerName = $"Player_{shortId}";
                
                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Player = GetPlayer(playerName),
                    Data = new Dictionary<string, DataObject>
                    {
                        { "Started", new DataObject(DataObject.VisibilityOptions.Public, "False") }
                    }
                };
                
                _currentLobby = await LobbyService.Instance.CreateLobbyAsync("GameLobby", _maxPlayers, options);
                
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(_maxPlayers);
                _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                
                var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
                
                // Fix: Use HostConnectionData from allocation
                utp.SetRelayServerData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData,
                    allocation.ConnectionData // Use ConnectionData as fallback for HostConnectionData
                );
                
                NetworkManager.Singleton.StartHost();
                
                OnJoinCodeReceived?.Invoke(_joinCode);
                OnLobbyJoined?.Invoke();
                
                Debug.Log($"Created lobby with join code: {_joinCode}");
                return _joinCode;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create lobby: {e.Message}");
                return null;
            }
        }
        
        public async Task JoinLobby(string joinCode)
        {
            try
            {
                string playerId = AuthenticationService.Instance.PlayerId;
                string shortId = playerId.Length > 6 ? playerId.Substring(0, 6) : playerId;
                string playerName = $"Player_{shortId}";
                
                JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
                {
                    Player = GetPlayer(playerName)
                };
                
                _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, options);
                _joinCode = joinCode;
                
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                
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
                Debug.Log($"Joined lobby with code: {joinCode}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to join lobby: {e.Message}");
            }
        }
        
        public async Task LeaveLobby()
        {
            if (_currentLobby != null)
            {
                try
                {
                    await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, AuthenticationService.Instance.PlayerId);
                    _currentLobby = null;
                    _joinCode = null;
                    
                    if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient))
                    {
                        NetworkManager.Singleton.Shutdown();
                    }
                    
                    OnLobbyLeft?.Invoke();
                    Debug.Log("Left lobby");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to leave lobby: {e.Message}");
                }
            }
        }
        
        public async Task StartGame()
        {
            if (_currentLobby != null && NetworkManager.Singleton.IsHost)
            {
                try
                {
                    await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions
                    {
                        Data = new Dictionary<string, DataObject>
                        {
                            { "Started", new DataObject(DataObject.VisibilityOptions.Public, "True") }
                        }
                    });
                    
                    NetworkManager.Singleton.SceneManager.LoadScene("Space", UnityEngine.SceneManagement.LoadSceneMode.Single);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to start game: {e.Message}");
                }
            }
        }
        
        private async Task HeartbeatLobbyAsync()
        {
            if (_currentLobby != null && NetworkManager.Singleton.IsHost)
            {
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Heartbeat failed: {e.Message}");
                }
            }
        }
        
        private async Task UpdateLobbyDataAsync()
        {
            if (_currentLobby != null)
            {
                try
                {
                    int playerCount = _currentLobby.Players?.Count ?? 0;
                    UpdateLobbyOptions options = new UpdateLobbyOptions
                    {
                        Data = new Dictionary<string, DataObject>
                        {
                            { "PlayerCount", new DataObject(DataObject.VisibilityOptions.Public, playerCount.ToString()) }
                        }
                    };
                    
                    _currentLobby = await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, options);
                    OnLobbyUpdated?.Invoke(_currentLobby);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to update lobby: {e.Message}");
                }
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
    }
}