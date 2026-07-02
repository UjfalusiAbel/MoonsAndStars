using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Network
{
    public class NetworkManagerSetup : MonoBehaviour
    {
        [SerializeField] private NetworkManager _networkManager;

        private void Awake()
        {
            if (_networkManager == null)
            {
                _networkManager = GetComponent<NetworkManager>();
            }

            if (_networkManager == null)
            {
                _networkManager = gameObject.AddComponent<NetworkManager>();
            }

            if (_networkManager.GetComponent<UnityTransport>() == null)
            {
                gameObject.AddComponent<UnityTransport>();
            }

            _networkManager.NetworkConfig.EnableSceneManagement = true;

            DontDestroyOnLoad(gameObject);
        }
    }
}