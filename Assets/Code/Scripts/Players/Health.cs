using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Players
{
public class Health : NetworkBehaviour
    {
        public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>(100, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                CurrentHealth.Value = 100;
            }
        }

        public void TakeDamage(int damageAmount)
        {
            if (!IsServer) return;

            CurrentHealth.Value = Mathf.Max(0, CurrentHealth.Value - damageAmount);
            Debug.Log($"[HEALTH LOG] {gameObject.name} took {damageAmount} damage. HP remaining: {CurrentHealth.Value}");

            if (CurrentHealth.Value <= 0)
            {
                HandleDeath();
            }
        }

        private void HandleDeath()
        {
            if (!IsServer) return;

            Debug.LogWarning($"[DEATH EVENT] {gameObject.name} dropped to 0 HP. Initiating game cleanup and returning to Bootstrap.");

            NetworkManager.Singleton.SceneManager.LoadScene("Bootstrap", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}