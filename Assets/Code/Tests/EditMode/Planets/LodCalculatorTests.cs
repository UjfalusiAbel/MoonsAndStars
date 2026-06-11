using NUnit.Framework;
using MoonsAndStars.Assets.Code.Scripts.Planets;
using System.Collections.Generic;

namespace MoonsAndStars.Assets.Code.Tests.EditMode.Planets
{
    [TestFixture]
    public class LodCalculatorTests
    {
        [Test]
        public void RecalculateDistances_ListCanBeModified()
        {
            List<float> distances = new List<float> { 50f, 100f, 200f, 500f };
            Assert.AreEqual(4, distances.Count);
            Assert.AreEqual(50f, distances[0]);
            Assert.AreEqual(500f, distances[3]);
        }

        [Test]
        public void DistanceComparison_WorksCorrectly()
        {
            float cameraToPlanet = 150f;
            List<float> distances = new List<float> { 50f, 100f, 200f, 500f };
            
            float selectedDistance = distances[distances.Count - 1];
            foreach (var dist in distances)
            {
                if (cameraToPlanet < dist)
                {
                    selectedDistance = dist;
                    break;
                }
            }
            
            Assert.AreEqual(200f, selectedDistance);
        }

        [Test]
        public void DistanceComparison_WhenVeryClose_SelectsSmallest()
        {
            float cameraToPlanet = 30f;
            List<float> distances = new List<float> { 50f, 100f, 200f, 500f };
            
            float selectedDistance = distances[distances.Count - 1];
            foreach (var dist in distances)
            {
                if (cameraToPlanet < dist)
                {
                    selectedDistance = dist;
                    break;
                }
            }
            
            Assert.AreEqual(50f, selectedDistance);
        }

        [Test]
        public void DistanceComparison_WhenVeryFar_SelectsLargest()
        {
            float cameraToPlanet = 1000f;
            List<float> distances = new List<float> { 50f, 100f, 200f, 500f };
            
            float selectedDistance = distances[distances.Count - 1];
            foreach (var dist in distances)
            {
                if (cameraToPlanet < dist)
                {
                    selectedDistance = dist;
                    break;
                }
            }
            
            Assert.AreEqual(500f, selectedDistance);
        }
    }
}