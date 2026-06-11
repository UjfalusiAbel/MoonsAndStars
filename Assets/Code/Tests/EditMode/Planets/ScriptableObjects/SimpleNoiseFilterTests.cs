using NUnit.Framework;
using MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models.Noise;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Tests.EditMode.Planets.ScriptableObjects
{
    [TestFixture]
    public class SimpleNoiseFilterTests
    {
        private SimpleNoiseFilter _filter;
        private NoiseDetails _details;

        [SetUp]
        public void SetUp()
        {
            _filter = ScriptableObject.CreateInstance<SimpleNoiseFilter>();
            _details = new NoiseDetails
            {
                numberOfLayers = 3,
                baseRoughness = 1f,
                roughness = 2f,
                strength = 1f,
                persistance = 0.5f,
                center = Vector3.zero,
                minValue = 0f
            };
        }

        [Test]
        public void EvaluatePoint_ReturnsValueWithinRange()
        {
            float result = _filter.EvaluatePoint(Vector3.zero, _details);
            Assert.IsTrue(result >= -1f && result <= 1f, $"Value {result} outside expected range");
        }

        [Test]
        public void EvaluatePoint_DifferentPoints_ReturnDifferentValues()
        {
            float val1 = _filter.EvaluatePoint(new Vector3(0.1f, 0.2f, 0.3f), _details);
            float val2 = _filter.EvaluatePoint(new Vector3(10f, 20f, 30f), _details);
            Assert.AreNotEqual(val1, val2, "Different points should produce different noise values");
        }

        [Test]
        public void EvaluatePoint_ZeroLayers_ReturnsZero()
        {
            _details.numberOfLayers = 0;
            float result = _filter.EvaluatePoint(Vector3.zero, _details);
            Assert.AreEqual(0f, result);
        }
    }
}