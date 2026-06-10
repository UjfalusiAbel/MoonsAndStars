using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.Enums;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.Helpers
{
    public class EnemyMovement : MonoBehaviour
    {
        [Header("Spaceship Movement")]
        [SerializeField] private float _maxSpeed = 50f;
        [SerializeField] private float _acceleration = 5f;
        [SerializeField] private float _turnRate = 120f;

        [Header("Combat Settings")]
        [SerializeField] private float _combatSpeed = 35f;
        [SerializeField] private float _combatRange = 25f;

        private Rigidbody _rb;
        private Transform _chaseTarget;
        private Vector3 _moveTargetPosition;
        private bool _isActive;
        private MovementMode _currentMode = MovementMode.Idle;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.linearDamping = 0f;
            _rb.angularDamping = 0.5f;
            _rb.mass = 100f;
        }

        private void FixedUpdate()
        {
            if (!_isActive) return;

            switch (_currentMode)
            {
                case MovementMode.ChaseTarget:
                    UpdateChase();
                    break;
                case MovementMode.MoveToPosition:
                    UpdateMoveToPosition();
                    break;
                case MovementMode.Idle:
                    ApplyBraking();
                    break;
            }

            if (_rb.linearVelocity.magnitude > _maxSpeed)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * _maxSpeed;
            }
        }

        private void UpdateChase()
        {
            if (_chaseTarget == null)
            {
                StopMoving();
                return;
            }

            Vector3 directionToTarget = (_chaseTarget.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, _chaseTarget.position);
            
            RotateTowards(directionToTarget);
            
            float targetSpeed;
            if (distance <= _combatRange)
            {
                targetSpeed = _combatSpeed;
            }
            else
            {
                targetSpeed = _maxSpeed;
            }
            
            Vector3 desiredVelocity = transform.forward * targetSpeed;
            Vector3 force = (desiredVelocity - _rb.linearVelocity) * _acceleration;
            _rb.AddForce(force, ForceMode.Acceleration);
        }

        private void UpdateMoveToPosition()
        {
            Vector3 directionToTarget = (_moveTargetPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, _moveTargetPosition);

            RotateTowards(directionToTarget);

            if (distance < 5f)
            {
                StopMoving();
                return;
            }

            Vector3 desiredVelocity = transform.forward * _maxSpeed;
            Vector3 force = (desiredVelocity - _rb.linearVelocity) * _acceleration;
            _rb.AddForce(force, ForceMode.Acceleration);
        }

        private void ApplyBraking()
        {
            Vector3 brakingForce = -_rb.linearVelocity * _acceleration;
            _rb.AddForce(brakingForce, ForceMode.Acceleration);
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction == Vector3.zero) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnRate * Time.fixedDeltaTime);
        }

        public void StartMoving() => _isActive = true;

        public void ChaseTarget(Transform target)
        {
            if (target == null) return;
            _chaseTarget = target;
            _currentMode = MovementMode.ChaseTarget;
            _isActive = true;
        }

        public void SetMoveDirection(Vector3 worldPosition)
        {
            _moveTargetPosition = worldPosition;
            _currentMode = MovementMode.MoveToPosition;
            _isActive = true;
            _chaseTarget = null;
        }

        public void FaceTarget(Transform target)
        {
            if (target == null) 
            {
                return;
            }

            Vector3 direction = (target.position - transform.position).normalized;
            RotateTowards(direction);
            
            if (_currentMode != MovementMode.ChaseTarget)
            {
                _currentMode = MovementMode.Idle;
            }
        }

        public void StopMoving()
        {
            _currentMode = MovementMode.Idle;
            _isActive = false;
            _chaseTarget = null;
        }

        public float GetCurrentSpeed() => _rb.linearVelocity.magnitude;
        public Vector3 GetVelocity() => _rb.linearVelocity;
    }
}