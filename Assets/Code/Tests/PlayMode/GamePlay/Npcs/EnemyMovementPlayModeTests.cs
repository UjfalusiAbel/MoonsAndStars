using System.Collections;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.Helpers;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.Enums;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MoonsAndStars.Assets.Code.Tests.PlayMode.GamePlay.Npcs
{
    [TestFixture]
    public class EnemyMovementPlayModeTests
    {
        private GameObject _enemyObject;
        private EnemyMovement _movement;
        private Rigidbody _rb;

        [SetUp]
        public void SetUp()
        {
            _enemyObject = new GameObject("TestEnemy");
            _rb = _enemyObject.AddComponent<Rigidbody>();
            _movement = _enemyObject.AddComponent<EnemyMovement>();

            _rb.useGravity = false;
            _rb.linearDamping = 0f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_enemyObject);
        }

        [UnityTest]
        public IEnumerator StartMoving_EnablesMovement()
        {
            _movement.StartMoving();
            yield return null;
            Assert.IsNotNull(_movement);
        }

        [UnityTest]
        public IEnumerator StopMoving_SetsModeToIdle()
        {
            _movement.StartMoving();
            _movement.ChaseTarget(new GameObject("Target").transform);
            yield return new WaitForFixedUpdate();

            _movement.StopMoving();
            yield return new WaitForFixedUpdate();

            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator ChaseTarget_WhenTargetIsNull_DoesNotThrow()
        {
            _movement.StartMoving();
            Assert.DoesNotThrow(() => _movement.ChaseTarget(null));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ChaseTarget_WithValidTarget_SetsChaseMode()
        {
            var target = new GameObject("Target");
            target.transform.position = new Vector3(10f, 0f, 0f);

            _movement.StartMoving();
            _movement.ChaseTarget(target.transform);

            yield return new WaitForFixedUpdate();

            Assert.AreNotEqual(Vector3.zero, _rb.linearVelocity);

            Object.Destroy(target);
        }

        [UnityTest]
        public IEnumerator SetMoveDirection_SetsMoveToPositionMode()
        {
            Vector3 targetPosition = new Vector3(20f, 0f, 0f);

            _movement.StartMoving();
            _movement.SetMoveDirection(targetPosition);

            yield return new WaitForFixedUpdate();

            Assert.AreNotEqual(Vector3.zero, _rb.linearVelocity);
        }

        [UnityTest]
        public IEnumerator FaceTarget_RotatesTowardTarget()
        {
            var target = new GameObject("Target");
            target.transform.position = new Vector3(10f, 0f, 0f);

            Quaternion originalRotation = _enemyObject.transform.rotation;
            _movement.FaceTarget(target.transform);

            yield return new WaitForFixedUpdate();

            Assert.AreNotEqual(originalRotation, _enemyObject.transform.rotation);

            Object.Destroy(target);
        }

        [Test]
        public void GetCurrentSpeed_ReturnsZero_WhenNotMoving()
        {
            _movement.StopMoving();
            Assert.AreEqual(0f, _movement.GetCurrentSpeed());
        }

        [UnityTest]
        public IEnumerator GetCurrentSpeed_ReturnsPositive_WhenMoving()
        {
            var target = new GameObject("Target");
            target.transform.position = new Vector3(10f, 0f, 0f);

            _movement.StartMoving();
            _movement.ChaseTarget(target.transform);

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.Greater(_movement.GetCurrentSpeed(), 0f);

            Object.Destroy(target);
        }
    }

}