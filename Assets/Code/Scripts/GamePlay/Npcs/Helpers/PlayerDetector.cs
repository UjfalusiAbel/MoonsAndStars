using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.Helpers
{
    public class PlayerDetector : MonoBehaviour
    {
        [SerializeField] private float _detectionRange = 200f;
        [SerializeField] private LayerMask _playerLayer;
        [Range(60, 360)]
        [SerializeField] private float _detectionAngle = 360f;
        [SerializeField] private bool _useLineOfSight = true;
        [SerializeField] private LayerMask _obstacleMask; 

        private List<GameObject> _detectedPlayers = new List<GameObject>();
        private float _lastDetectionTime;
        private float _detectionInterval = 0.2f;

        public void Detect()
        {
            if (Time.time - _lastDetectionTime < _detectionInterval) return;
            _lastDetectionTime = Time.time;

            _detectedPlayers.Clear();

            Collider[] hits = Physics.OverlapSphere(transform.position, _detectionRange, _playerLayer);

            foreach (var hit in hits)
            {
                GameObject player = hit.transform.gameObject;
                
                if (player == null) continue;
                
                Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
                float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                
                bool angleValid = _detectionAngle >= 360f ||  Vector3.Angle(transform.forward, directionToPlayer) <= _detectionAngle / 2f;
                
                if (!angleValid) continue;
                
                bool hasLineOfSight = true;
                if (_useLineOfSight && _obstacleMask != 0)
                {
                    Vector3 rayStart = transform.position + transform.forward * 1f;
                    Ray ray = new Ray(rayStart, directionToPlayer);
                    RaycastHit rayHit;
                    
                    if (Physics.Raycast(ray, out rayHit, distanceToPlayer, _obstacleMask))
                    {
                        if (!rayHit.transform.CompareTag("Player"))
                        {
                            hasLineOfSight = false;
                        }
                    }
                }
                
                if (hasLineOfSight)
                {
                    _detectedPlayers.Add(player);
                    Debug.LogWarning(player.name);
                }
            }
        }

        public List<GameObject> GetDetectedPlayers() => _detectedPlayers;
        public List<GameObject> GetPlayersInRange() => _detectedPlayers;

        public GameObject GetClosestPlayer()
        {
            GameObject closest = null;
            float closestDistance = float.MaxValue;

            foreach (var player in _detectedPlayers)
            {
                if (player == null) 
                {
                    continue;
                }
                
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = player;
                }
            }

            return closest;
        }
        
        public bool HasAnyPlayerDetected() => _detectedPlayers.Count > 0;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
        }
    }
}