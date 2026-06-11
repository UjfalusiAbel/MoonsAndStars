using NUnit.Framework;
using MoonsAndStars.Assets.Code.Scripts.Planets;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models.Noise;
using MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Tests.EditMode.Planets
{
    [TestFixture]
    public class PlanetTerrainGeneratorTests
    {
        private PlanetTerrainGenerator _generator;
        private List<NoiseConfiguration> _noiseConfigs;

        [SetUp]
        public void SetUp()
        {
            _generator = PlanetTerrainGenerator.Singleton;
            _noiseConfigs = new List<NoiseConfiguration>();
        }

        [Test]
        public void EvaluateHeight_WithEmptyConfig_ReturnsZero()
        {
            float height = _generator.EvaluateHeight(Vector3.zero, null);
            Assert.AreEqual(0f, height);
        }

        [Test]
        public void EvaluateHeight_WithEmptyList_ReturnsZero()
        {
            float height = _generator.EvaluateHeight(Vector3.zero, _noiseConfigs);
            Assert.AreEqual(0f, height);
        }

        [Test]
        public void EvaluateHeight_ReturnsFiniteValue()
        {
            float height = _generator.EvaluateHeight(Vector3.zero, _noiseConfigs);
            Assert.IsTrue(float.IsFinite(height));
        }
    }
}