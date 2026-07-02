using Unity.Netcode; // Required for RPCs
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.Helpers
{
    public class EnemyShooting : NetworkBehaviour
    {
        [SerializeField] private float _fireRate = 2f; 
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _projectileSpeed = 50f;

        public float GetFireRate => _fireRate;

        public void Shoot(Vector3 direction)
        {
            if (!IsServer) return;

            if (_projectilePrefab == null || _firePoint == null)
            {
                Debug.LogWarning($"{name} missing projectile prefab or fire point!");
                return;
            }

            SpawnServerProjectile(direction);
            PlayShootEffectsClientRpc(direction, _firePoint.position);
        }

        private void SpawnServerProjectile(Vector3 direction)
        {
            GameObject projectile = Instantiate(_projectilePrefab, _firePoint.position, Quaternion.LookRotation(direction));
            
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * _projectileSpeed;
            }
        }

        [ClientRpc]
        private void PlayShootEffectsClientRpc(Vector3 direction, Vector3 originPosition)
        {
            if (IsServer) return; 
            GameObject cosmeticProjectile = Instantiate(_projectilePrefab, originPosition, Quaternion.LookRotation(direction));
            Rigidbody rb = cosmeticProjectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * _projectileSpeed;
            }
        }
    }
}