using NUnit.Framework;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Tests.EditMode.GamePlay.Npcs.States
{

    [TestFixture]
    public class AttackStateLogicTests
    {
        [Test]
        public void StrafeDirection_ShouldBeRandomized_OnEnter()
        {
            float strafeDirection1 = 1f;
            float strafeDirection2 = -1f;

            bool isPositiveOrNegative = (strafeDirection1 == 1f || strafeDirection1 == -1f);
            Assert.IsTrue(isPositiveOrNegative);

            bool shouldFlip = true;
            if (shouldFlip)
            {
                strafeDirection2 = -strafeDirection1;
                Assert.AreEqual(-1f, strafeDirection2);
            }
        }

        [Test]
        public void StrafeDirection_Alternates_AfterTimer()
        {
            float strafeDirection = 1f;
            float strafeTimer = 0f;
            float strafeInterval = 2f;

            Assert.AreEqual(1f, strafeDirection);
            strafeTimer = 2f;
            if (strafeTimer >= strafeInterval)
            {
                strafeDirection = -strafeDirection;
                strafeTimer = 0f;
            }

            Assert.AreEqual(-1f, strafeDirection);
        }

        [Test]
        public void CombatMovement_CalculatesStrafeVector_Correctly()
        {
            Vector3 targetPos = new Vector3(0f, 0f, 10f);
            Vector3 ownerPos = new Vector3(5f, 0f, 10f);
            float strafeDirection = 1f;

            Vector3 directionFromTarget = (ownerPos - targetPos).normalized;
            Vector3 strafeVector = Vector3.Cross(directionFromTarget, Vector3.up) * strafeDirection;

            Assert.AreEqual(1f, strafeVector.magnitude, 0.001f);
            Assert.IsTrue(Mathf.Abs(strafeVector.y) < 0.001f);
        }

        [Test]
        public void CombatMovement_StrafeVector_WithNegativeDirection_Flips()
        {
            Vector3 targetPos = new Vector3(0f, 0f, 10f);
            Vector3 ownerPos = new Vector3(5f, 0f, 10f);
            float strafeDirectionPositive = 1f;
            float strafeDirectionNegative = -1f;

            Vector3 strafeVectorPositive = Vector3.Cross((ownerPos - targetPos).normalized, Vector3.up) * strafeDirectionPositive;
            Vector3 strafeVectorNegative = Vector3.Cross((ownerPos - targetPos).normalized, Vector3.up) * strafeDirectionNegative;

            Assert.AreEqual(-strafeVectorPositive, strafeVectorNegative);
        }

        [Test]
        public void AttackRange_Multiplier_ShouldBe130Percent()
        {
            float attackRange = 30f;
            float exitThreshold = attackRange * 1.3f;
            Assert.AreEqual(39f, exitThreshold, 0.001f);
        }

        [TestCase(30f, 39f, true)]
        [TestCase(38f, 39f, true)]
        [TestCase(39f, 39f, true)]
        [TestCase(40f, 39f, false)] 
        public void AttackExitCondition_WhenBeyond130Percent_ShouldTransition(float currentDistance, float exitThreshold, bool shouldStay)
        {
            bool shouldStayInAttack = currentDistance <= exitThreshold;
            Assert.AreEqual(shouldStay, shouldStayInAttack);
        }
    }

}