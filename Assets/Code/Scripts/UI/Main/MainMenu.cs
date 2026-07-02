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
        [SerializeField] private GameObject _namePanel;
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _lobbyPanel;
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_InputField _codeInput;
        private string _userName = "";

        private UnityGameServicesManager _gameServices;

        private void Start()
        {
            _gameServices = UnityGameServicesManager.Instance;
            _choicePanel.SetActive(false);
            _lobbyPanel.SetActive(false);
            _namePanel.SetActive(false);
            _mainPanel.SetActive(true);
        }

        public void GoToNamePanel()
        {
            _mainPanel.SetActive(false);
            _namePanel.SetActive(true);
        }

        public void GoToChoicePanel()
        {
            ReadName();
            if (string.IsNullOrWhiteSpace(_userName))
            {
                Debug.LogWarning("Cannot proceed: Username cannot be blank!");
                return; 
            }

            // Save username globally so LobbyUIManager can read it safely
            if (_gameServices != null)
            {
                _gameServices.SetLocalPlayerName(_userName);
            }

            _namePanel.SetActive(false);
            _choicePanel.SetActive(true);
        }

        public void HostGame()
        {
            // Handled dynamically by LobbyUIManager panel transitions
        }

        public void JoinGame()
        {
            // Handled dynamically by LobbyUIManager panel transitions
        }

        public void ExitNamePanel()
        {
            _namePanel.SetActive(false);
            _mainPanel.SetActive(true);
        }

        public void ExitChoicePanel()
        {
            _choicePanel.SetActive(false);
            _namePanel.SetActive(true);
        }

        public void ReadName()
        {
            if (_nameInput != null)
            {
                _userName = _nameInput.text.Trim(); 
            }
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}