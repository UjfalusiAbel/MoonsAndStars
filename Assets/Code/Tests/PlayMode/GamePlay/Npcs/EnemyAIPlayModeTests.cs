using System.Collections;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.Helpers;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MoonsAndStars.Assets.Code.Tests.PlayMode.GamePlay.Npcs
{
    [TestFixture]
    public class EnemyAIPlayModeTests
    {
        private GameObject _enemyObject;
        private EnemyAI _enemyAI;

        [SetUp]
        public void SetUp()
        {
            _enemyObject = new GameObject("TestEnemy");
            _enemyObject.AddComponent<Rigidbody>();
            _enemyObject.AddComponent<PlayerDetector>();
            _enemyObject.AddComponent<EnemyMovement>();
            _enemyObject.AddComponent<EnemyShooting>();
            _enemyAI = _enemyObject.AddComponent<EnemyAI>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_enemyObject);
        }

        [UnityTest]
        public IEnumerator Awake_InitializesComponents()
        {
            yield return null;

            Assert.IsNotNull(_enemyAI.GetMovement);
            Assert.IsNotNull(_enemyAI.GetShooting);
            Assert.IsNotNull(_enemyAI.GetPlayerDetector);
        }

        [Test]
        public void GetAttackRange_ReturnsSerializedValue()
        {
            Assert.AreEqual(30f, _enemyAI.GetAttackRange);
        }

        [Test]
        public void GetChaseRange_ReturnsSerializedValue()
        {
            Assert.AreEqual(150f, _enemyAI.GetChaseRange);
        }

        [Test]
        public void GetSearchDuration_ReturnsSerializedValue()
        {
            Assert.AreEqual(5f, _enemyAI.GetSearchDuration);
        }

        [UnityTest]
        public IEnumerator Start_SetsInitialStateToSearch()
        {
            yield return null;

            Assert.IsNotNull(_enemyAI.CurrentState);
            Assert.IsInstanceOf<SearchState>(_enemyAI.CurrentState);
        }

        [Test]
        public void ChangeState_WhenCalled_UpdatesCurrentState()
        {
            var mockState = new SearchState(_enemyObject, _enemyAI);
            _enemyAI.ChangeState(mockState);

            Assert.AreEqual(mockState, _enemyAI.CurrentState);
        }

        [UnityTest]
        public IEnumerator GetClosestPlayer_WhenNoPlayers_ReturnsNull()
        {
            yield return null;

            var closest = _enemyAI.GetClosestPlayer();
            Assert.IsNull(closest);
        }

        [UnityTest]
        public IEnumerator HasPlayersInRange_WhenNoPlayers_ReturnsFalse()
        {
            yield return null;

            Assert.IsFalse(_enemyAI.HasPlayersInRange());
        }
    }

}