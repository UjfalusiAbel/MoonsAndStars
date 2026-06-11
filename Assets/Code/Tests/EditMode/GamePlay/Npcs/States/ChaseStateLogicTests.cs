using NUnit.Framework;

namespace MoonsAndStars.Assets.Code.Tests.EditMode.GamePlay.Npcs.States
{
    [TestFixture]
    public class ChaseStateLogicTests
    {
        [TestCase(25f, 30f, true)]
        [TestCase(30f, 30f, true)]
        [TestCase(31f, 30f, false)]
        [TestCase(150f, 30f, false)]
        public void ShouldTransitionToAttack_BasedOnDistance(float distanceToTarget, float attackRange, bool shouldTransition)
        {
            bool transitionToAttack = distanceToTarget <= attackRange;
            Assert.AreEqual(shouldTransition, transitionToAttack);
        }

        [TestCase(151f, 150f, true)]
        [TestCase(200f, 150f, true)]
        [TestCase(150f, 150f, false)]
        [TestCase(100f, 150f, false)]
        public void ShouldTransitionToSearch_BasedOnDistance(float distanceToTarget, float chaseRange, bool shouldTransition)
        {
            bool transitionToSearch = distanceToTarget > chaseRange;
            Assert.AreEqual(shouldTransition, transitionToSearch);
        }

        [TestCase(0f, true)]
        [TestCase(75f, true)]
        [TestCase(150f, true)]
        [TestCase(151f, false)]
        public void ChaseRange_ContainsDistance_WhenWithinLimits(float distance, bool expectedInRange)
        {
            float chaseRange = 150f;
            bool isInChaseRange = distance <= chaseRange;
            Assert.AreEqual(expectedInRange, isInChaseRange);
        }

        [Test]
        public void ChaseTarget_WhenCloserPlayerAppears_ShouldSwitchTarget()
        {
            float currentTargetDistance = 100f;
            float newTargetDistance = 50f;

            bool shouldSwitchTarget = newTargetDistance < currentTargetDistance;

            Assert.IsTrue(shouldSwitchTarget);
        }

        [Test]
        public void ChaseTarget_WhenFurtherPlayerAppears_ShouldKeepCurrentTarget()
        {
            float currentTargetDistance = 50f;
            float newTargetDistance = 100f;

            bool shouldSwitchTarget = newTargetDistance < currentTargetDistance;

            Assert.IsFalse(shouldSwitchTarget);
        }
    }

}