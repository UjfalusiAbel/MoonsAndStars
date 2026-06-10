using System.Collections.Generic;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.Helpers;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.States
{
    public class SearchState : EnemyState
    {
        private PlayerDetector _detector;
        private Vector3 _wanderDirection;
        private float _wanderRadius = 150f;
        private float _wanderCooldown;
        private float _detectionTimer;

        public SearchState(GameObject owner, EnemyAI enemyAI) : base(owner, enemyAI)
        {
            _detector = owner.GetComponent<PlayerDetector>();
        }

        public override void Enter()
        {
            Debug.Log($"{_owner.name} entered search");
            _wanderCooldown = 0f;
            _detectionTimer = 0f;
            GenerateNewWanderTarget();
            _enemyAI.GetMovement.SetMoveDirection(_wanderDirection);
            _enemyAI.GetMovement.StartMoving();
        }

        public override void Exit()
        {
            _enemyAI.GetMovement.StopMoving();
            Debug.Log($"{_owner.name} exited search");
        }

        public override void Update()
        {
            // Detect players every frame
            _detector.Detect();

            // IMMEDIATELY transition if player detected
            if (_detector.HasAnyPlayerDetected())
            {
                GameObject closestPlayer = _detector.GetClosestPlayer();
                if (closestPlayer != null)
                {
                    float distance = Vector3.Distance(_owner.transform.position, closestPlayer.transform.position);
                    Debug.Log($"{_owner.name} detected player at {distance:F0} units! Switching to Chase");
                    _enemyAI.ChangeState(new ChaseState(_owner, _enemyAI));
                    return;
                }
            }

            // Update wander target periodically
            if (_wanderCooldown <= 0f)
            {
                GenerateNewWanderTarget();
                _enemyAI.GetMovement.SetMoveDirection(_wanderDirection);
                _wanderCooldown = Random.Range(3f, 6f);
            }
            else
            {
                _wanderCooldown -= Time.deltaTime;
            }
        }

        private void GenerateNewWanderTarget()
        {
            Vector2 randomCircle = Random.insideUnitCircle * _wanderRadius;
            _wanderDirection = _owner.transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        }
    }
}