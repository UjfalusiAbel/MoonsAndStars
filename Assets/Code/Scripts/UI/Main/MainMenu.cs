using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using MoonsAndStars.Assets.Code.Scripts.Multiplayer;

namespace MoonsAndStars.Assets.Code.Scripts.UI.Main
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _choicePanel;
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _lobbyPanel;
        [SerializeField] private TMP_InputField _codeInput;
        [SerializeField] private TMP_InputField _lobbyNameInput;
        [SerializeField] private UnityEngine.UI.Toggle _privateLobbyToggle;
        
        private UnityGameServicesManager _gameServices;
        
        private void Start()
        {
            _gameServices = UnityGameServicesManager.Instance;
            _choicePanel.SetActive(false);
            _lobbyPanel.SetActive(false);
        }
        
        public void Play()
        {
            _mainPanel.SetActive(false);
            _choicePanel.SetActive(true);
        }
        
        public void HostGame()
        {
            _choicePanel.SetActive(false);
            _lobbyPanel.SetActive(true);
            CreateLobby();
        }
        
        public void JoinGame()
        {
            if (!string.IsNullOrEmpty(_codeInput.text))
            {
                _choicePanel.SetActive(false);
                JoinLobby();
            }
        }
        
        public void ExitChoicePanel()
        {
            _choicePanel.SetActive(false);
            _mainPanel.SetActive(true);
        }
        
        public void ExitGame()
        {
            Application.Quit();
        }
        
        private async void CreateLobby()
        {
            if (_gameServices == null)
            {
                Debug.LogError("Game Services not initialized!");
                return;
            }
            
            string lobbyName = string.IsNullOrEmpty(_lobbyNameInput.text) ? "GameLobby" : _lobbyNameInput.text;
            bool isPrivate = _privateLobbyToggle != null && _privateLobbyToggle.isOn;
            
            string joinCode = await _gameServices.CreateLobby(isPrivate);
            
            if (!string.IsNullOrEmpty(joinCode))
            {
                Debug.Log($"Lobby created! Join Code: {joinCode}");
            }
        }
        
        private async void JoinLobby()
        {
            if (_gameServices == null)
            {
                Debug.LogError("Game Services not initialized!");
                return;
            }
            
            await _gameServices.JoinLobby(_codeInput.text.ToUpper());
        }
    }
}