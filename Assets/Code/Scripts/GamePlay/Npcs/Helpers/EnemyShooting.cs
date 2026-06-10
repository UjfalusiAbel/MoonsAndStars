using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.Helpers
{
    public class EnemyShooting : MonoBehaviour
    {
        [SerializeField] private float _fireRate = 2f; 
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _projectileSpeed = 50f;

        public float GetFireRate => _fireRate;

        public void Shoot(Vector3 direction)
        {
            if (_projectilePrefab == null || _firePoint == null)
            {
                Debug.LogWarning($"{name} missing projectile prefab or fire point!");
                return;
            }

            GameObject projectile = Instantiate(_projectilePrefab, _firePoint.position, Quaternion.LookRotation(direction));
            Rigidbody rb = projectile.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = direction * _projectileSpeed;
            }

        }
    }
}