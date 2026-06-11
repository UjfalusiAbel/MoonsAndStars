using System.Collections;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.Helpers;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.States;
using MoonsAndStars.Assets.Code.Tests.PlayMode.GamePlay.Npcs.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MoonsAndStars.Assets.Code.Tests.PlayMode.GamePlay.Npcs.States
{
    [TestFixture]
    public class StateTransitionPlayModeTests
    {
        private GameObject _enemyObject;
        private GameObject _player;
        private EnemyAI _enemyAI;
        private PlayerDetector _detector;

        [SetUp]
        public void SetUp()
        {
            _enemyObject = new GameObject("TestEnemy");
            _enemyObject.AddComponent<Rigidbody>();
            _detector = _enemyObject.AddComponent<PlayerDetector>();
            _detector.ConfigureForTesting();
            _enemyObject.AddComponent<EnemyMovement>();
            _enemyObject.AddComponent<EnemyShooting>();
            _enemyAI = _enemyObject.AddComponent<EnemyAI>();

            _player = new GameObject("TestPlayer");
            _player.tag = "Player";
            var boxCollider = _player.AddComponent<BoxCollider>();
            boxCollider.size = Vector3.one;
            
            var playerRb = _player.AddComponent<Rigidbody>();
            playerRb.useGravity = false;
            
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                _player.layer = playerLayer;
            }
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_enemyObject);
            Object.Destroy(_player);
        }

        [UnityTest]
        public IEnumerator SearchState_WhenPlayerDetected_TransitionsToChase()
        {
            _player.transform.position = _enemyObject.transform.position + Vector3.forward * 50f;
            
            _enemyAI.ChangeState(new SearchState(_enemyObject, _enemyAI));
            
            float timeout = 3f;
            float startTime = Time.time;
            
            while (Time.time - startTime < timeout)
            {
                _detector.Detect();
                _enemyAI.Update();
                yield return new WaitForSeconds(0.05f);
                
                if (_enemyAI.CurrentState is ChaseState)
                {
                    Assert.Pass();
                    yield break;
                }
            }
            
            Assert.Fail($"Never transitioned to ChaseState. Current state: {_enemyAI.CurrentState?.GetType().Name}");
        }

        [UnityTest]
        public IEnumerator ChaseState_WhenInAttackRange_TransitionsToAttack()
        {
            _player.transform.position = _enemyObject.transform.position + Vector3.forward * 20f;
            
            _enemyAI.ChangeState(new ChaseState(_enemyObject, _enemyAI));
            
            float timeout = 3f;
            float startTime = Time.time;
            
            while (Time.time - startTime < timeout)
            {
                _detector.Detect();
                _enemyAI.Update();
                yield return new WaitForSeconds(0.05f);
                
                if (_enemyAI.CurrentState is AttackState)
                {
                    Assert.Pass();
                    yield break;
                }
            }
            
            Assert.Fail($"Never transitioned to AttackState. Current state: {_enemyAI.CurrentState?.GetType().Name}");
        }

        [UnityTest]
        public IEnumerator ChaseState_WhenOutOfChaseRange_TransitionsToSearch()
        {
            _player.transform.position = _enemyObject.transform.position + Vector3.forward * 200f;
            
            _enemyAI.ChangeState(new ChaseState(_enemyObject, _enemyAI));
            
            float timeout = 3f;
            float startTime = Time.time;
            
            while (Time.time - startTime < timeout)
            {
                _detector.Detect();
                _enemyAI.Update();
                yield return new WaitForSeconds(0.05f);
                
                if (_enemyAI.CurrentState is SearchState)
                {
                    Assert.Pass();
                    yield break;
                }
            }
            
            Assert.Fail($"Never transitioned to SearchState. Current state: {_enemyAI.CurrentState?.GetType().Name}");
        }

        [UnityTest]
        public IEnumerator AttackState_WhenTargetLost_TransitionsToSearch()
        {
            _player.transform.position = _enemyObject.transform.position + Vector3.forward * 20f;
            
            _detector.Detect();
            _enemyAI.ChangeState(new AttackState(_enemyObject, _enemyAI));
            
            yield return new WaitForSeconds(0.3f);
            
            Object.Destroy(_player);
            
            float timeout = 3f;
            float startTime = Time.time;
            
            while (Time.time - startTime < timeout)
            {
                _detector.Detect();
                _enemyAI.Update();
                yield return new WaitForSeconds(0.05f);
                
                if (_enemyAI.CurrentState is SearchState)
                {
                    Assert.Pass();
                    yield break;
                }
            }
            
            Assert.Fail($"Never transitioned to SearchState. Current state: {_enemyAI.CurrentState?.GetType().Name}");
        }
        
        [UnityTest]
        public IEnumerator ChaseState_StaysInChase_WhenInChaseRangeButNotAttackRange()
        {
            _player.transform.position = _enemyObject.transform.position + Vector3.forward * 80f;
            
            _detector.Detect();
            _enemyAI.ChangeState(new ChaseState(_enemyObject, _enemyAI));
            
            yield return new WaitForSeconds(0.5f);
            
            for (int i = 0; i < 10; i++)
            {
                _detector.Detect();
                _enemyAI.Update();
                yield return new WaitForSeconds(0.05f);
            }
            
            bool isChaseState = _enemyAI.CurrentState is ChaseState;
            Assert.IsTrue(isChaseState, $"Expected ChaseState but got {_enemyAI.CurrentState?.GetType().Name}");
        }
        
        [UnityTest]
        public IEnumerator FullStateFlow_SearchToChaseToAttack()
        {
            _player.transform.position = _enemyObject.transform.position + Vector3.forward * 80f;
            
            _detector.Detect();
            _enemyAI.ChangeState(new SearchState(_enemyObject, _enemyAI));
            
            float timeout = 3f;
            float startTime = Time.time;
            
            while (Time.time - startTime < timeout)
            {
                _detector.Detect();
                _enemyAI.Update();
                yield return new WaitForSeconds(0.05f);
                
                if (_enemyAI.CurrentState is ChaseState)
                {
                    break;
                }
            }
            
            Assert.IsInstanceOf<ChaseState>(_enemyAI.CurrentState, "Should go from Search to Chase when player detected");
            
            _player.transform.position = _enemyObject.transform.position + Vector3.forward * 20f;
            
            startTime = Time.time;
            
            while (Time.time - startTime < timeout)
            {
                _detector.Detect();
                _enemyAI.Update();
                yield return new WaitForSeconds(0.05f);
                
                if (_enemyAI.CurrentState is AttackState)
                {
                    Assert.Pass();
                    yield break;
                }
            }
            
            Assert.Fail("Should go from Chase to Attack when in range");
        }
    }
}