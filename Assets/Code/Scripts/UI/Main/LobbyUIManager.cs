using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using MoonsAndStars.Assets.Code.Scripts.Multiplayer;

namespace MoonsAndStars.Assets.Code.Scripts.UI.Main
{
    public class LobbyUIManager : MonoBehaviour
    {
        [Header("Main Menu Panels")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _lobbyPanel;
        [SerializeField] private GameObject _choicePanel;
        
        [Header("Lobby Creation")]
        [SerializeField] private TMP_InputField _lobbyNameInput;
        [SerializeField] private Toggle _privateLobbyToggle;
        [SerializeField] private Button _createLobbyButton;
        
        [Header("Lobby Join")]
        [SerializeField] private TMP_InputField _joinCodeInput;
        [SerializeField] private Button _joinLobbyButton;
        
        [Header("Lobby Display")]
        [SerializeField] private GameObject _lobbyInfoPanel;
        [SerializeField] private TextMeshProUGUI _joinCodeText;
        [SerializeField] private TextMeshProUGUI _playerListText;
        [SerializeField] private TextMeshProUGUI _lobbyStatusText;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _leaveLobbyButton;
        
        [Header("Player List")]
        [SerializeField] private Transform _playerListContainer;
        [SerializeField] private GameObject _playerEntryPrefab;
        
        [Header("Loading")]
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private TextMeshProUGUI _loadingText;
        
        [Header("Error")]
        [SerializeField] private GameObject _errorPanel;
        [SerializeField] private TextMeshProUGUI _errorText;
        
        private UnityGameServicesManager _gameServices;
        private bool _isHost;
        
        private void Start()
        {
            _gameServices = UnityGameServicesManager.Instance;
            
            if (_gameServices == null)
            {
                ShowError("Game Services not initialized. Please restart the game.");
                return;
            }
            
            SetupUI();
            SubscribeEvents();
        }
        
        private void SetupUI()
        {
            _createLobbyButton.onClick.AddListener(OnCreateLobbyClicked);
            _joinLobbyButton.onClick.AddListener(OnJoinLobbyClicked);
            _startGameButton.onClick.AddListener(OnStartGameClicked);
            _leaveLobbyButton.onClick.AddListener(OnLeaveLobbyClicked);
            
            _lobbyInfoPanel.SetActive(false);
            _loadingPanel.SetActive(false);
            _errorPanel.SetActive(false);
            _lobbyPanel.SetActive(false);
        }
        
        private void SubscribeEvents()
        {
            _gameServices.OnSignedIn += OnSignedIn;
            _gameServices.OnJoinCodeReceived += OnJoinCodeReceived;
            _gameServices.OnLobbyJoined += OnLobbyJoined;
            _gameServices.OnLobbyLeft += OnLobbyLeft;
            _gameServices.OnLobbyUpdated += OnLobbyUpdated;
        }
        
        private void OnDestroy()
        {
            if (_gameServices != null)
            {
                _gameServices.OnSignedIn -= OnSignedIn;
                _gameServices.OnJoinCodeReceived -= OnJoinCodeReceived;
                _gameServices.OnLobbyJoined -= OnLobbyJoined;
                _gameServices.OnLobbyLeft -= OnLobbyLeft;
                _gameServices.OnLobbyUpdated -= OnLobbyUpdated;
            }
        }
        
        private void OnSignedIn()
        {
            Debug.Log("Signed in to Game Services");
        }
        
        private async void OnCreateLobbyClicked()
        {
            ShowLoading("Creating lobby...");
            string joinCode = await _gameServices.CreateLobby(_privateLobbyToggle.isOn);
            HideLoading();
            
            if (string.IsNullOrEmpty(joinCode))
            {
                ShowError("Failed to create lobby. Please try again.");
            }
        }
        
        private async void OnJoinLobbyClicked()
        {
            if (string.IsNullOrEmpty(_joinCodeInput.text))
            {
                ShowError("Please enter a join code.");
                return;
            }
            
            ShowLoading("Joining lobby...");
            await _gameServices.JoinLobby(_joinCodeInput.text.ToUpper());
            HideLoading();
        }
        
        private void OnStartGameClicked()
        {
            if (_isHost)
            {
                ShowLoading("Starting game...");
                _gameServices.StartGame();
            }
        }
        
        private async void OnLeaveLobbyClicked()
        {
            ShowLoading("Leaving lobby...");
            await _gameServices.LeaveLobby();
            HideLoading();
            
            _lobbyPanel.SetActive(false);
            _choicePanel.SetActive(true);
        }
        
        private void OnJoinCodeReceived(string joinCode)
        {
            _joinCodeText.text = $"Join Code: {joinCode}";
            _isHost = true;
            _startGameButton.gameObject.SetActive(true);
        }
        
        private void OnLobbyJoined()
        {
            _lobbyPanel.SetActive(true);
            _choicePanel.SetActive(false);
            _mainPanel.SetActive(false);
            _lobbyInfoPanel.SetActive(true);
            
            _isHost = _gameServices.CurrentLobby?.HostId == 
                Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
            
            _startGameButton.gameObject.SetActive(_isHost);
            UpdatePlayerList();
        }
        
        private void OnLobbyLeft()
        {
            _lobbyInfoPanel.SetActive(false);
            _isHost = false;
            
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }
        
        private void OnLobbyUpdated(Lobby lobby)
        {
            UpdatePlayerList();
            
            bool gameStarted = lobby.Data != null && 
                              lobby.Data.ContainsKey("Started") && 
                              lobby.Data["Started"].Value == "True";
            
            if (gameStarted && _isHost)
            {
                _lobbyStatusText.text = "Game Starting...";
            }
        }
        
        private void UpdatePlayerList()
        {
            if (_gameServices.CurrentLobby == null) return;
            
            foreach (Transform child in _playerListContainer)
            {
                Destroy(child.gameObject);
            }
            
            int playerCount = _gameServices.CurrentLobby.Players?.Count ?? 0;
            
            foreach (var player in _gameServices.CurrentLobby.Players)
            {
                GameObject entry = Instantiate(_playerEntryPrefab, _playerListContainer);
                TextMeshProUGUI playerNameText = entry.GetComponentInChildren<TextMeshProUGUI>();
                
                string playerName = player.Data != null && player.Data.ContainsKey("PlayerName") ?
                    player.Data["PlayerName"].Value : player.Id.Substring(0, 6);
                
                bool isHost = player.Id == _gameServices.CurrentLobby.HostId;
                playerNameText.text = isHost ? $"{playerName} (Host)" : playerName;
            }
            
            _playerListText.text = $"Players: {playerCount}/{_gameServices.CurrentLobby.MaxPlayers}";
        }
        
        private void ShowLoading(string message)
        {
            _loadingText.text = message;
            _loadingPanel.SetActive(true);
        }
        
        private void HideLoading()
        {
            _loadingPanel.SetActive(false);
        }
        
        private void ShowError(string message)
        {
            _errorText.text = message;
            _errorPanel.SetActive(true);
            Invoke(nameof(HideError), 3f);
        }
        
        private void HideError()
        {
            _errorPanel.SetActive(false);
        }
    }
}