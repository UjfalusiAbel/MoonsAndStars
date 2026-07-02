using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using MoonsAndStars.Assets.Code.Scripts.Multiplayer; 

namespace MoonsAndStars.Assets.Code.Scripts.Network
{
    public class SpaceSpawnManager : MonoBehaviour
    {
        [Header("Prefab Setup")]
        [SerializeField] private GameObject _playerPrefab; // Drag your Spaceship prefab here
        [SerializeField] private GameObject _npcPrefab;    // Drag your AI/NPC prefab here

        [Header("Spawn Points")]
        [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
        private int _spawnIndex = 0;

        private void Start()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Destroy(this);
                return;
            }

            NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject == null)
                {
                    SpawnPlayerShip(client.ClientId);
                }
            }

            SpawnLobbyNPCs();
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
            }
        }

        private void HandleSceneEvent(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
            {
                ulong clientId = sceneEvent.ClientId;
                
                if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject == null)
                {
                    SpawnPlayerShip(clientId);
                }
            }
        }

        private void SpawnPlayerShip(ulong clientId)
        {
            if (_playerPrefab == null)
            {
                Debug.LogError("Player Prefab is missing from SpaceSpawnManager!", this);
                return;
            }

            Vector3 spawnPos = GetNextSpawnPosition(out Quaternion spawnRot);

            GameObject playerShip = Instantiate(_playerPrefab, spawnPos, spawnRot);
            NetworkObject netObj = playerShip.GetComponent<NetworkObject>();

            if (netObj != null)
            {
                netObj.SpawnAsPlayerObject(clientId, true);
                Debug.Log($"[SPAWN SUCCESS] Successfully spawned player ship for Client: {clientId}");
            }
            else
            {
                Debug.LogError("Your Spaceship prefab is missing a NetworkObject component! Cannot spawn.", playerShip);
            }
        }

        private void SpawnLobbyNPCs()
        {
            var ugs = UnityGameServicesManager.Instance;
            if (ugs == null || _npcPrefab == null) 
            {
                Debug.LogWarning("[SERVER SPAWN] Cannot handle NPC instantiation. Either Services Manager is missing or NPC Prefab slot is empty.");
                return;
            }

            int npcCountToSpawn = 0;

            if (NetworkManager.Singleton.IsHost)
            {
                npcCountToSpawn = ugs.LocalHostNpcCountConfig;
            }
            else if (ugs.CurrentLobby != null && ugs.CurrentLobby.Data != null && ugs.CurrentLobby.Data.ContainsKey("NpcCount"))
            {
                int.TryParse(ugs.CurrentLobby.Data["NpcCount"].Value, out npcCountToSpawn);
            }

            Debug.Log($"[SERVER SPAWN] Instantiating {npcCountToSpawn} computer controlled ships safely bypassing network race updates.");

            for (int i = 0; i < npcCountToSpawn; i++)
            {
                Vector3 spawnPos = GetNextSpawnPosition(out Quaternion spawnRot);
                GameObject npcShip = Instantiate(_npcPrefab, spawnPos, spawnRot);
                
                NetworkObject netObj = npcShip.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn(true);
                    Debug.Log($"[NPC SPAWNED] Successfully instantiated computer drone #{i + 1} at scene position {spawnPos}");
                }
                else
                {
                    Debug.LogError("[NPC SPAWN ERROR] Your NPC Prefab is missing a NetworkObject component configuration!", _npcPrefab);
                }
            }
        }

        private Vector3 GetNextSpawnPosition(out Quaternion rotation)
        {
            Vector3 pos = Vector3.zero;
            rotation = Quaternion.identity;

            if (_spawnPoints != null && _spawnPoints.Count > 0)
            {
                Transform point = _spawnPoints[_spawnIndex];
                pos = point.position;
                rotation = point.rotation;
                _spawnIndex = (_spawnIndex + 1) % _spawnPoints.Count;
            }
            return pos;
        }
    }
}