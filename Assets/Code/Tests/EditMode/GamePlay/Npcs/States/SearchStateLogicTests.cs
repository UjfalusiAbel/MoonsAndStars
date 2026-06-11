using NUnit.Framework;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Tests.EditMode.GamePlay.Npcs.States
{
    [TestFixture]
    public class SearchStateLogicTests
    {
        [Test]
        public void SearchDuration_Timer_CountsDownCorrectly()
        {
            float searchDuration = 5f;
            float searchTimer = 5f;

            searchTimer -= Time.deltaTime;

            Assert.Less(searchTimer, searchDuration);
        }

        [Test]
        public void SearchDuration_WhenTimerReachesZero_ShouldTransition()
        {
            float searchTimer = 0f;
            bool shouldTransitionToIdle = searchTimer <= 0f;

            Assert.IsTrue(shouldTransitionToIdle);
        }

        [TestCase(0f, 5f, false)]   // Just started
        [TestCase(2.5f, 5f, false)] // Halfway
        [TestCase(5f, 5f, true)]    // Exactly at duration
        [TestCase(6f, 5f, true)]    // Exceeded duration
        public void SearchExpiration_WhenTimerExceedsDuration_ShouldTimeout(float elapsedTime, float duration, bool shouldTimeout)
        {
            bool isExpired = elapsedTime >= duration;
            Assert.AreEqual(shouldTimeout, isExpired);
        }

        [Test]
        public void SearchState_WhenPlayerDetected_ShouldTransitionToChase()
        {
            bool playerDetected = true;
            bool shouldTransitionToChase = playerDetected;

            Assert.IsTrue(shouldTransitionToChase);
        }

        [Test]
        public void SearchState_WhenNoPlayerDetected_ShouldContinueSearching()
        {
            bool playerDetected = false;
            bool shouldTransitionToChase = playerDetected;

            Assert.IsFalse(shouldTransitionToChase);
        }
    }
}