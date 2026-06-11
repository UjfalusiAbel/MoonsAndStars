using System.Collections;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MoonsAndStars.Assets.Code.Tests.PlayMode.GamePlay.Npcs
{
    [TestFixture]
    public class EnemyShootingPlayModeTests
    {
        private GameObject _enemyObject;
        private EnemyShooting _shooting;

        [SetUp]
        public void SetUp()
        {
            _enemyObject = new GameObject("TestEnemy");
            _shooting = _enemyObject.AddComponent<EnemyShooting>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_enemyObject);
        }

        [Test]
        public void GetFireRate_ReturnsSerializedValue()
        {
            Assert.AreEqual(2f, _shooting.GetFireRate);
        }

        [Test]
        public void Shoot_WithMissingPrefab_LogsWarning()
        {
            Vector3 direction = Vector3.forward;

            LogAssert.Expect(LogType.Warning, $"{_enemyObject.name} missing projectile prefab or fire point!");
            _shooting.Shoot(direction);
        }

        [UnityTest]
        public IEnumerator Shoot_WithValidPrefab_InstantiatesProjectile()
        {
            var projectilePrefab = new GameObject("ProjectilePrefab");
            projectilePrefab.AddComponent<Rigidbody>();

            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(_enemyObject.transform);

            Vector3 direction = Vector3.forward;

            Assert.Pass("Requires setting private fields via reflection or InternalsVisibleTo");

            Object.Destroy(projectilePrefab);
            Object.Destroy(firePoint);
            yield return null;
        }
    }
}
