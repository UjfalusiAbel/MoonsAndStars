using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.States
{
    public class ChaseState : EnemyState
    {
        private GameObject _target;

        public ChaseState(GameObject owner, EnemyAI enemyAI) : base(owner, enemyAI) { }

        public override void Enter()
        {
            Debug.Log($"{_owner.name} entered chase");
            _target = _enemyAI.GetClosestPlayer();

            if (_target == null)
            {
                _enemyAI.ChangeState(new SearchState(_owner, _enemyAI));
                return;
            }
            
            _enemyAI.GetMovement.ChaseTarget(_target.transform);
            _enemyAI.GetMovement.StartMoving();
        }

        public override void Exit()
        {
            Debug.Log($"{_owner.name} exited chase");
        }

        public override void Update()
        {
            if (_target == null)
            {
                _enemyAI.ChangeState(new SearchState(_owner, _enemyAI));
                return;
            }

            float distanceToTarget = Vector3.Distance(_owner.transform.position, _target.transform.position);

            // Switch to attack when in range
            if (distanceToTarget <= _enemyAI.GetAttackRange)
            {
                Debug.Log($"{_owner.name} in attack range ({distanceToTarget:F1}), switching to Attack");
                _enemyAI.ChangeState(new AttackState(_owner, _enemyAI));
                return;
            }

            // Lost target, go search
            if (distanceToTarget > _enemyAI.GetChaseRange)
            {
                Debug.Log($"{_owner.name} lost target, switching to Search");
                _enemyAI.ChangeState(new SearchState(_owner, _enemyAI));
                return;
            }

            // Update target in case a closer player appears
            GameObject closestPlayer = _enemyAI.GetClosestPlayer();
            if (closestPlayer != null && closestPlayer != _target)
            {
                _target = closestPlayer;
                _enemyAI.GetMovement.ChaseTarget(_target.transform);
            }
            
            // Continue chasing
            _enemyAI.GetMovement.ChaseTarget(_target.transform);
        }
    }
}