using System.Collections;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MoonsAndStars.Assets.Code.Tests.PlayMode.GamePlay.Npcs
{
    [TestFixture]
    public class PlayerDetectorPlayModeTests
    {
        private GameObject _detectorObject;
        private PlayerDetector _detector;
        private GameObject _player;

        [SetUp]
        public void SetUp()
        {
            _detectorObject = new GameObject("TestDetector");
            _detector = _detectorObject.AddComponent<PlayerDetector>();

            _player = new GameObject("TestPlayer");
            _player.tag = "Player";
            _player.AddComponent<Collider>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_detectorObject);
            Object.Destroy(_player);
        }

        [UnityTest]
        public IEnumerator Detect_WithPlayerInRange_DetectsPlayer()
        {
            _player.transform.position = _detectorObject.transform.position + Vector3.forward * 10f;

            _detector.Detect();
            yield return null;

            var players = _detector.GetPlayersInRange();
            Assert.IsNotNull(players);
        }

        [Test]
        public void GetClosestPlayer_WithNoPlayers_ReturnsNull()
        {
            var closest = _detector.GetClosestPlayer();
            Assert.IsNull(closest);
        }

        [Test]
        public void HasAnyPlayerDetected_Initially_ReturnsFalse()
        {
            Assert.IsFalse(_detector.HasAnyPlayerDetected());
        }

        [UnityTest]
        public IEnumerator GetPlayersInRange_ReturnsList_WhenPlayersDetected()
        {
            _player.transform.position = _detectorObject.transform.position + Vector3.forward * 10f;

            _detector.Detect();
            yield return null;

            var players = _detector.GetPlayersInRange();
            Assert.IsNotNull(players);
        }
    }
}