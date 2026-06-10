using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.States
{
    public class AttackState : EnemyState
    {
        private GameObject _target;
        private float _attackCooldown;
        private float _strafeDirection = 1f;
        private float _strafeTimer;
        private float _orbitDirection = 1f;

        public AttackState(GameObject owner, EnemyAI enemyAI) : base(owner, enemyAI) { }

        public override void Enter()
        {
            Debug.Log($"{_owner.name} entered attack");
            _attackCooldown = 0f;
            _strafeTimer = 0f;
            _strafeDirection = Random.value > 0.5f ? 1f : -1f;
            _orbitDirection = Random.value > 0.5f ? 1f : -1f;
            _target = _enemyAI.GetClosestPlayer();

            if (_target == null)
            {
                _enemyAI.ChangeState(new SearchState(_owner, _enemyAI));
                return;
            }
            
            _enemyAI.GetMovement.ChaseTarget(_target.transform);
        }

        public override void Exit()
        {
            Debug.Log($"{_owner.name} exited attack");
        }

        public override void Update()
        {
            if (_target == null)
            {
                _enemyAI.ChangeState(new SearchState(_owner, _enemyAI));
                return;
            }

            float distanceToTarget = Vector3.Distance(_owner.transform.position, _target.transform.position);

            if (distanceToTarget > _enemyAI.GetAttackRange * 1.3f && distanceToTarget <= _enemyAI.GetChaseRange)
            {
                Debug.Log($"{_owner.name} too far, returning to chase");
                _enemyAI.ChangeState(new ChaseState(_owner, _enemyAI));
                return;
            }

            if (distanceToTarget > _enemyAI.GetChaseRange)
            {
                _enemyAI.ChangeState(new SearchState(_owner, _enemyAI));
                return;
            }

            GameObject closestPlayer = _enemyAI.GetClosestPlayer();
            if (closestPlayer != null && closestPlayer != _target)
            {
                _target = closestPlayer;
            }

            UpdateCombatMovement(distanceToTarget);
            
            _enemyAI.GetMovement.FaceTarget(_target.transform);
            
            if (distanceToTarget <= _enemyAI.GetAttackRange)
            {
                if (_attackCooldown <= 0f)
                {
                    ShootAtTarget();
                    _attackCooldown = 1f / _enemyAI.GetShooting.GetFireRate;
                }
                else
                {
                    _attackCooldown -= Time.deltaTime;
                }
            }
        }

        private void UpdateCombatMovement(float distanceToTarget)
        {
            _strafeTimer += Time.deltaTime;
            
            if (_strafeTimer >= 2f)
            {
                _strafeDirection = -_strafeDirection;
                _strafeTimer = 0f;
            }
            
            Vector3 targetPos = _target.transform.position;
            Vector3 directionFromTarget = (_owner.transform.position - targetPos).normalized;
            Vector3 strafeVector = Vector3.Cross(directionFromTarget, Vector3.up) * _strafeDirection;
            Vector3 strafeTarget = targetPos + directionFromTarget * _enemyAI.GetAttackRange + strafeVector * 10f;

            _enemyAI.GetMovement.ChaseTarget(_target.transform);
        }

        private void ShootAtTarget()
        {
            if (_target == null) 
            {
                return;
            }
            
            Rigidbody targetRb = _target.GetComponent<Rigidbody>();
            Vector3 targetVelocity = targetRb != null ? targetRb.linearVelocity : Vector3.zero;
            float distance = Vector3.Distance(_owner.transform.position, _target.transform.position);
            float projectileSpeed = 50f;
            float leadTime = distance / projectileSpeed;
            Vector3 predictedPosition = _target.transform.position + targetVelocity * leadTime;
            
            Vector3 shootDirection = (predictedPosition - _owner.transform.position).normalized;
            
            Debug.Log($"{_owner.name} FIRING at {_target.name}!");
            _enemyAI.GetShooting.Shoot(shootDirection);
        }
    }
}