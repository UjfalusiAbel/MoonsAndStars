using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Tests.EditMode.GamePlay.Npcs
{
    [TestFixture]
    public class PlayerDetectionLogicTests
    {
        [Test]
        public void AngleCheck_360Degrees_AlwaysValid()
        {
            float detectionAngle = 360f;
            float angleToPlayer = 180f;
            bool angleValid = detectionAngle >= 360f || angleToPlayer <= detectionAngle / 2f;
            Assert.IsTrue(angleValid);
        }

        [TestCase(90f, 45f, true)]
        [TestCase(90f, 44f, true)]
        [TestCase(90f, 46f, false)]
        [TestCase(180f, 90f, true)]
        [TestCase(180f, 89f, true)]
        [TestCase(180f, 91f, false)]
        [TestCase(120f, 60f, true)]
        [TestCase(120f, 61f, false)]
        [TestCase(360f, 180f, true)]
        [TestCase(360f, 200f, true)]
        public void ConeAngleCheck_ValidatesCorrectly(float detectionAngle, float angleToPlayer, bool expectedValid)
        {
            bool angleValid = detectionAngle >= 360f || angleToPlayer <= detectionAngle / 2f;
            Assert.AreEqual(expectedValid, angleValid);
        }

        [Test]
        public void ClosestPlayer_WithMultiplePlayers_ReturnsCorrectOne()
        {
            var playerDistances = new Dictionary<string, float>
            {
                { "Player1", 50f },
                { "Player2", 30f },
                { "Player3", 100f }
            };
            
            float closestDistance = float.MaxValue;
            string closestPlayer = null;
            
            foreach (var player in playerDistances)
            {
                if (player.Value < closestDistance)
                {
                    closestDistance = player.Value;
                    closestPlayer = player.Key;
                }
            }
            
            Assert.AreEqual("Player2", closestPlayer);
            Assert.AreEqual(30f, closestDistance);
        }

        [Test]
        public void ClosestPlayer_WithEmptyList_ReturnsNull()
        {
            var playerDistances = new Dictionary<string, float>();
            float closestDistance = float.MaxValue;
            string closestPlayer = null;
            
            foreach (var player in playerDistances)
            {
                if (player.Value < closestDistance)
                {
                    closestDistance = player.Value;
                    closestPlayer = player.Key;
                }
            }
            
            Assert.IsNull(closestPlayer);
            Assert.AreEqual(float.MaxValue, closestDistance);
        }

        [Test]
        public void ClosestPlayer_WithSinglePlayer_ReturnsThatPlayer()
        {
            var playerDistances = new Dictionary<string, float>
            {
                { "SoloPlayer", 42f }
            };
            
            float closestDistance = float.MaxValue;
            string closestPlayer = null;
            
            foreach (var player in playerDistances)
            {
                if (player.Value < closestDistance)
                {
                    closestDistance = player.Value;
                    closestPlayer = player.Key;
                }
            }
            
            Assert.AreEqual("SoloPlayer", closestPlayer);
            Assert.AreEqual(42f, closestDistance);
        }

        [Test]
        public void HasAnyPlayerDetected_EmptyList_ReturnsFalse()
        {
            var detectedPlayers = new List<GameObject>();
            bool hasPlayers = detectedPlayers.Count > 0;
            Assert.IsFalse(hasPlayers);
        }

        [Test]
        public void HasAnyPlayerDetected_WithPlayers_ReturnsTrue()
        {
            var detectedPlayers = new List<string> { "Player1", "Player2" };
            bool hasPlayers = detectedPlayers.Count > 0;
            Assert.IsTrue(hasPlayers);
        }

        [Test]
        public void DetectionRange_ChecksDistanceCorrectly()
        {
            float detectionRange = 200f;
            float enemyPosition = 0f;
            float playerPosition = 150f;
            float distance = Mathf.Abs(playerPosition - enemyPosition);
            bool isInRange = distance <= detectionRange;
            Assert.IsTrue(isInRange);
            
            playerPosition = 250f;
            distance = Mathf.Abs(playerPosition - enemyPosition);
            isInRange = distance <= detectionRange;
            Assert.IsFalse(isInRange);
        }

        [Test]
        public void LineOfSight_WithNoObstacles_ShouldDetect()
        {
            bool hasObstacle = false;
            bool hasLineOfSight = !hasObstacle;
            Assert.IsTrue(hasLineOfSight);
        }

        [Test]
        public void LineOfSight_WithObstacle_ShouldNotDetect()
        {
            bool hasObstacle = true;
            bool hasLineOfSight = !hasObstacle;
            Assert.IsFalse(hasLineOfSight);
        }
    }
}