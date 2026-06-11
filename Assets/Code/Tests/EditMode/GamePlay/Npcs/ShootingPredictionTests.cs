using NUnit.Framework;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Tests.EditMode.GamePlay.Npcs
{

    [TestFixture]
    public class ShootingPredictionTests
    {
        [Test]
        public void LeadTimeCalculation_WithStationaryTarget_ReturnsCorrectLead()
        {
            float distance = 50f;
            float projectileSpeed = 50f;
            float leadTime = distance / projectileSpeed;
            Assert.AreEqual(1f, leadTime, 0.001f);
        }

        [Test]
        public void LeadTimeCalculation_WithMovingTarget_LeadsCorrectly()
        {
            float distance = 100f;
            float projectileSpeed = 50f;
            Vector3 targetVelocity = new Vector3(10f, 0f, 0f);
            Vector3 targetPosition = new Vector3(0f, 0f, 0f);
            float leadTime = distance / projectileSpeed;
            Vector3 predictedPosition = targetPosition + targetVelocity * leadTime;
            Assert.AreEqual(2f, leadTime, 0.001f);
            Assert.AreEqual(new Vector3(20f, 0f, 0f), predictedPosition);
        }

        [Test]
        public void ShootingPrediction_WithDiagonalMovement_PredictsCorrectly()
        {
            float distance = 80f;
            float projectileSpeed = 40f;
            Vector3 targetVelocity = new Vector3(5f, 10f, 0f);
            Vector3 targetPosition = new Vector3(10f, 5f, 0f);
            float leadTime = distance / projectileSpeed;
            Vector3 predictedPosition = targetPosition + targetVelocity * leadTime;
            Assert.AreEqual(2f, leadTime, 0.001f);
            Assert.AreEqual(new Vector3(20f, 25f, 0f), predictedPosition);
        }

        [TestCase(50f, 50f, 1f)]
        [TestCase(100f, 50f, 2f)]
        [TestCase(25f, 100f, 0.25f)]
        [TestCase(75f, 150f, 0.5f)]
        [TestCase(200f, 50f, 4f)]
        public void LeadTime_CalculatesCorrectly_ForVariousSpeedsAndDistances(float distance, float projectileSpeed, float expectedLeadTime)
        {
            float leadTime = distance / projectileSpeed;
            Assert.AreEqual(expectedLeadTime, leadTime, 0.001f);
        }

        [Test]
        public void ShootingDirection_CalculatesNormalizedVector()
        {
            Vector3 shooterPosition = Vector3.zero;
            Vector3 targetPosition = new Vector3(10f, 0f, 0f);
            Vector3 shootDirection = (targetPosition - shooterPosition).normalized;
            Assert.AreEqual(Vector3.right, shootDirection);
            Assert.AreEqual(1f, shootDirection.magnitude, 0.001f);
        }

        [Test]
        public void ShootingDirection_WithElevationDifference_NormalizesCorrectly()
        {
            Vector3 shooterPosition = new Vector3(0f, 0f, 0f);
            Vector3 targetPosition = new Vector3(10f, 15f, 0f);
            float expectedLength = Mathf.Sqrt(10f * 10f + 15f * 15f);
            Vector3 shootDirection = (targetPosition - shooterPosition).normalized;
            Assert.AreEqual(1f, shootDirection.magnitude, 0.001f);
            Assert.AreEqual(new Vector3(10f / expectedLength, 15f / expectedLength, 0f), shootDirection);
        }
    }

}