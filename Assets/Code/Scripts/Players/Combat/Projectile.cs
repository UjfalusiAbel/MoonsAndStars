using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs;
using MoonsAndStars.Assets.Code.Scripts.Players.Input;
using Unity.Netcode;
using UnityEngine;
namespace MoonsAndStars.Assets.Code.Scripts.Players.Combat
{

public class Projectile : NetworkBehaviour
    {
        [SerializeField] private int _damageValue = 10;

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            Health targetHealth = other.GetComponentInParent<Health>();
            if (targetHealth == null)
            {
                targetHealth = other.GetComponent<Health>();
            }

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(_damageValue);

                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}