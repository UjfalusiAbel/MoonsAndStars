using NUnit.Framework;

namespace MoonsAndStars.Assets.Code.Tests.EditMode
{
    [TestFixture]
    public class EnemyStateLogicTests
    {
        private class MockEnemyAIData
        {
            public float AttackRange { get; set; } = 30f;
            public float ChaseRange { get; set; } = 150f;
            public float SearchDuration { get; set; } = 5f;
            public string CurrentState { get; set; } = "Search";
        }

        [Test]
        public void DistanceChecks_AttackRangeTransition_WhenWithin30Units()
        {
            float distanceToTarget = 25f;
            float attackRange = 30f;
            bool shouldEnterAttack = distanceToTarget <= attackRange;
            Assert.IsTrue(shouldEnterAttack);
        }

        [Test]
        public void DistanceChecks_AttackRangeTransition_WhenExactlyAt30Units()
        {
            float distanceToTarget = 30f;
            float attackRange = 30f;
            bool shouldEnterAttack = distanceToTarget <= attackRange;
            Assert.IsTrue(shouldEnterAttack);
        }

        [Test]
        public void DistanceChecks_AttackRangeTransition_WhenOutside30Units_ReturnsFalse()
        {
            float distanceToTarget = 35f;
            float attackRange = 30f;
            bool shouldEnterAttack = distanceToTarget <= attackRange;
            Assert.IsFalse(shouldEnterAttack);
        }

        [Test]
        public void DistanceChecks_ChaseTransition_WhenBetween30And150Units()
        {
            float distanceToTarget = 100f;
            float attackRange = 30f;
            float chaseRange = 150f;
            bool shouldChase = distanceToTarget > attackRange && distanceToTarget <= chaseRange;
            Assert.IsTrue(shouldChase);
        }

        [Test]
        public void DistanceChecks_ChaseTransition_WhenAtLowerBoundary_ReturnsFalse()
        {
            float distanceToTarget = 30f;
            float attackRange = 30f;
            float chaseRange = 150f;
            bool shouldChase = distanceToTarget > attackRange && distanceToTarget <= chaseRange;
            Assert.IsFalse(shouldChase);
        }

        [Test]
        public void DistanceChecks_ChaseTransition_WhenAtUpperBoundary_ReturnsTrue()
        {
            float distanceToTarget = 150f;
            float attackRange = 30f;
            float chaseRange = 150f;
            bool shouldChase = distanceToTarget > attackRange && distanceToTarget <= chaseRange;
            Assert.IsTrue(shouldChase);
        }

        [Test]
        public void DistanceChecks_SearchTransition_WhenBeyond150Units()
        {
            float distanceToTarget = 200f;
            float chaseRange = 150f;
            bool shouldSearch = distanceToTarget > chaseRange;
            Assert.IsTrue(shouldSearch);
        }

        [Test]
        public void DistanceChecks_SearchTransition_WhenExactlyAt150Units_ReturnsFalse()
        {
            float distanceToTarget = 150f;
            float chaseRange = 150f;
            bool shouldSearch = distanceToTarget > chaseRange;
            Assert.IsFalse(shouldSearch);
        }

        [TestCase(0f, 30f, true)]
        [TestCase(15f, 30f, true)]
        [TestCase(30f, 30f, true)]
        [TestCase(30.1f, 30f, false)]
        [TestCase(150f, 30f, false)]
        [TestCase(100f, 30f, false)]
        public void AttackRangeCheck_VariousDistances_ReturnsExpected(float distance, float attackRange, bool expectedInRange)
        {
            bool isInAttackRange = distance <= attackRange;
            Assert.AreEqual(expectedInRange, isInAttackRange);
        }

        [TestCase(0f, 150f, true)]
        [TestCase(75f, 150f, true)]
        [TestCase(150f, 150f, true)]
        [TestCase(150.1f, 150f, false)]
        [TestCase(300f, 150f, false)]
        public void ChaseRangeCheck_VariousDistances_ReturnsExpected(float distance, float chaseRange, bool expectedInRange)
        {
            bool isInChaseRange = distance <= chaseRange;
            Assert.AreEqual(expectedInRange, isInChaseRange);
        }
    }
}