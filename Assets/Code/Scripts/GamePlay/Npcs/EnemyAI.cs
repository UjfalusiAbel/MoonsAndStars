using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.Helpers;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.States;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.States.Interfaces;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerDetector))]
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(EnemyShooting))]
    public class EnemyAI : MonoBehaviour
    {
        private IEnemyState _currentState;

        [Header("State Settings")]
        [SerializeField] private float _searchDuration = 5f;
        [SerializeField] private float _attackRange = 30f;   // Attack from 30 units
        [SerializeField] private float _chaseRange = 150f;   // Chase from up to 150 units

        private PlayerDetector _playerDetector;
        private EnemyMovement _movement;
        private EnemyShooting _shooting;

        public float GetSearchDuration => _searchDuration;
        public float GetAttackRange => _attackRange;
        public float GetChaseRange => _chaseRange;
        public PlayerDetector GetPlayerDetector => _playerDetector;
        public EnemyMovement GetMovement => _movement;
        public EnemyShooting GetShooting => _shooting;

        public IEnemyState CurrentState => _currentState;

        private void Awake()
        {
            _playerDetector = GetComponent<PlayerDetector>();
            _movement = GetComponent<EnemyMovement>();
            _shooting = GetComponent<EnemyShooting>();

            if (_movement == null)
                _movement = gameObject.AddComponent<EnemyMovement>();

            if (_shooting == null)
                _shooting = gameObject.AddComponent<EnemyShooting>();
        }

        private void Start()
        {
            _currentState = new SearchState(gameObject, this);
            _currentState.Enter();
        }

        public void Update()
        {
            if (_currentState != null)
            {
                _currentState.Update();
            }
        }

        public void ChangeState(IEnemyState newState)
        {
            if (_currentState != null)
            {
                _currentState.Exit();
            }

            _currentState = newState;
            _currentState.Enter();
        }

        public GameObject GetClosestPlayer()
        {
            return _playerDetector.GetClosestPlayer();
        }

        public bool HasPlayersInRange()
        {
            return _playerDetector.GetPlayersInRange().Count > 0;
        }
    }
}