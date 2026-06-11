using NUnit.Framework;
using MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models.Noise;
using UnityEngine;


namespace MoonsAndStars.Assets.Code.Tests.EditMode.Planets.ScriptableObjects
{
    [TestFixture]
    public class RidgedNoiseFilterTests
    {
        private RidgedNoiseFilter _filter;
        private NoiseDetails _details;

        [SetUp]
        public void SetUp()
        {
            _filter = ScriptableObject.CreateInstance<RidgedNoiseFilter>();
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
            _filter.ridgeOffset = 1f;
            _filter.ridgeSharpness = 1f;
        }

        [Test]
        public void EvaluatePoint_ReturnsNonNegativeValue()
        {
            float result = _filter.EvaluatePoint(Vector3.zero, _details);
            Assert.GreaterOrEqual(result, 0f);
        }

        [Test]
        public void RidgeOffset_ChangesOutput()
        {
            _filter.ridgeOffset = 0.5f;
            float result1 = _filter.EvaluatePoint(Vector3.zero, _details);
            _filter.ridgeOffset = 1.5f;
            float result2 = _filter.EvaluatePoint(Vector3.zero, _details);
            Assert.AreNotEqual(result1, result2);
        }
    }

}