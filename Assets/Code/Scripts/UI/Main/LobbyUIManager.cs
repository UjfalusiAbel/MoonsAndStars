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
        [SerializeField] private Button _createLobbyButton;

        [Header("Lobby Join")]
        [SerializeField] private TMP_InputField _joinCodeInput;
        [SerializeField] private Button _joinLobbyButton;

        [Header("Lobby Display")]
        [SerializeField] private GameObject _lobbyInfoPanel;
        [SerializeField] private TextMeshProUGUI _joinCodeText;
        [SerializeField] private TextMeshProUGUI _playerListText;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _leaveLobbyButton;
        [SerializeField] private TMP_InputField _npcCountInput;

        [Header("Player List")]
        [SerializeField] private Transform _playerListContainer;
        [SerializeField] private GameObject _playerEntryPrefab;

        [Header("Loading")]
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private TextMeshProUGUI _loadingText;

        [Header("Error Display")]
        [SerializeField] private GameObject _errorPanel;
        [SerializeField] private TextMeshProUGUI _errorText;

        private UnityGameServicesManager _gameServices;
        private bool _isHost;

        private void Start()
        {
            _gameServices = UnityGameServicesManager.Instance;

            if (_gameServices != null)
            {
                _gameServices.OnLobbyJoined += HandleLobbyJoined;
                _gameServices.OnLobbyLeft += HandleLobbyLeft;
                _gameServices.OnLobbyUpdated += OnLobbyUpdated;
            }

            if (_createLobbyButton != null) _createLobbyButton.onClick.AddListener(OnCreateLobbyPressed);
            if (_joinLobbyButton != null) _joinLobbyButton.onClick.AddListener(OnJoinLobbyPressed);
            if (_startGameButton != null) _startGameButton.onClick.AddListener(OnStartGamePressed);
            if (_leaveLobbyButton != null) _leaveLobbyButton.onClick.AddListener(OnLeaveLobbyPressed);

            HideLoading();
            if (_errorPanel != null) _errorPanel.SetActive(false);
        }

        private async void OnCreateLobbyPressed()
        {
            ShowLoading("Creating network lobby...");
            _isHost = true;
            string code = await _gameServices.CreateLobby(_gameServices.LocalPlayerName, false);
            HideLoading();

            if (string.IsNullOrEmpty(code))
            {
                ShowError("Lobby creation failed!");
            }
        }

        private async void OnJoinLobbyPressed()
        {
            if (_joinCodeInput == null || string.IsNullOrWhiteSpace(_joinCodeInput.text))
            {
                ShowError("Please enter a valid Join Code!");
                return;
            }

            ShowLoading("Connecting to session...");
            _isHost = false;
            await _gameServices.JoinLobby(_joinCodeInput.text.Trim().ToUpper(), _gameServices.LocalPlayerName);
            HideLoading();
        }

        private async void OnStartGamePressed()
        {
            if (!_isHost) return;

            int npcCount = 0;
            if (_npcCountInput != null && !string.IsNullOrWhiteSpace(_npcCountInput.text))
            {
                int.TryParse(_npcCountInput.text, out npcCount);
            }

            ShowLoading("Synchronizing environment with clients...");
            await _gameServices.StartGame(npcCount);
        }

        private async void OnLeaveLobbyPressed()
        {
            ShowLoading("Leaving session...");
            await _gameServices.LeaveLobby();
            HideLoading();
        }

        private void HandleLobbyJoined()
        {
            if (_mainPanel != null) _mainPanel.SetActive(false);
            if (_choicePanel != null) _choicePanel.SetActive(false);
            if (_lobbyPanel != null) _lobbyPanel.SetActive(true);
            if (_lobbyInfoPanel != null) _lobbyInfoPanel.SetActive(true);

            if (_startGameButton != null)
            {
                _startGameButton.gameObject.SetActive(_isHost);
            }
            
            if (_npcCountInput != null)
            {
                _npcCountInput.interactable = _isHost;
            }

            UpdatePlayerList();
            InvokeRepeating(nameof(UpdateJoinCodeDisplay), 0.5f, 2f);
        }

        private void HandleLobbyLeft()
        {
            CancelInvoke(nameof(UpdateJoinCodeDisplay));
            if (_lobbyPanel != null) _lobbyPanel.SetActive(false);
            if (_lobbyInfoPanel != null) _lobbyInfoPanel.SetActive(false);
            if (_mainPanel != null) _mainPanel.SetActive(true);
        }

        private void OnLobbyUpdated(Lobby lobby)
        {
            UpdatePlayerList();

            if (!_isHost && lobby.Data != null && lobby.Data.ContainsKey("NpcCount"))
            {
                if (_npcCountInput != null)
                {
                    _npcCountInput.text = lobby.Data["NpcCount"].Value;
                }
            }

            // CRITICAL FIX: Removed manual application-side scene switching.
            // Netcode handles client scene migration automatically.
            if (lobby.Data != null && lobby.Data.ContainsKey("Started") && lobby.Data["Started"].Value == "True")
            {
                ShowLoading("Match started! Syncing with host...");
            }
        }

        private void UpdatePlayerList()
        {
            if (_gameServices.CurrentLobby == null) return;

            foreach (Transform child in _playerListContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var player in _gameServices.CurrentLobby.Players)
            {
                GameObject entry = Instantiate(_playerEntryPrefab, _playerListContainer);
                var textComponent = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null && player.Data != null && player.Data.ContainsKey("PlayerName"))
                {
                    textComponent.text = player.Data["PlayerName"].Value;
                }
            }
        }

        private void UpdateJoinCodeDisplay()
        {
            if (_gameServices.CurrentLobby != null && _joinCodeText != null)
            {
                _joinCodeText.text = $"Join Code: {_gameServices.JoinCode}";
            }
        }

        private void ShowLoading(string message)
        {
            if (_loadingText != null) _loadingText.text = message;
            if (_loadingPanel != null) _loadingPanel.SetActive(true);
        }

        private void HideLoading()
        {
            if (_loadingPanel != null) _loadingPanel.SetActive(false);
        }

        private void ShowError(string message)
        {
            if (_errorText != null) _errorText.text = message;
            if (_errorPanel != null) _errorPanel.SetActive(true);
            Invoke(nameof(HideError), 3f);
        }

        private void HideError()
        {
            if (_errorPanel != null) _errorPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_gameServices != null)
            {
                _gameServices.OnLobbyJoined -= HandleLobbyJoined;
                _gameServices.OnLobbyLeft -= HandleLobbyLeft;
                _gameServices.OnLobbyUpdated -= OnLobbyUpdated;
            }
        }
    }
}