using NUnit.Framework;
using MoonsAndStars.Assets.Code.Scripts.Planets;
using System.Collections.Generic;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Tests.EditMode.Planets
{
    [TestFixture]
    public class PlanetColorizerTests
    {
        private PlanetColorizer _colorizer;

        [SetUp]
        public void SetUp()
        {
            GameObject go = new GameObject();
            _colorizer = go.AddComponent<PlanetColorizer>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_colorizer.gameObject);
        }

        [Test]
        public void PlanetType_Enum_HasCorrectValues()
        {
            var values = System.Enum.GetValues(typeof(PlanetColorizer.PlanetType));
            Assert.AreEqual(7, values.Length);
        }

        [Test]
        public void ApplyPreset_EarthLike_AddsColorPairs()
        {
            _colorizer.ApplyPreset(PlanetColorizer.PlanetType.EarthLike);
            var field = typeof(PlanetColorizer).GetField("m_heightColorPairs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pairs = field.GetValue(_colorizer) as List<PlanetColorizer.HeightColorPair>;
            Assert.IsNotNull(pairs);
            Assert.Greater(pairs.Count, 0);
        }

        [Test]
        public void ApplyPreset_Desert_AddsColorPairs()
        {
            _colorizer.ApplyPreset(PlanetColorizer.PlanetType.Desert);
            var field = typeof(PlanetColorizer).GetField("m_heightColorPairs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pairs = field.GetValue(_colorizer) as List<PlanetColorizer.HeightColorPair>;
            Assert.IsNotNull(pairs);
            Assert.Greater(pairs.Count, 0);
        }

        [Test]
        public void ApplyPreset_Ice_AddsColorPairs()
        {
            _colorizer.ApplyPreset(PlanetColorizer.PlanetType.Ice);
            var field = typeof(PlanetColorizer).GetField("m_heightColorPairs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pairs = field.GetValue(_colorizer) as List<PlanetColorizer.HeightColorPair>;
            Assert.IsNotNull(pairs);
            Assert.Greater(pairs.Count, 0);
        }

        [Test]
        public void ApplyPreset_Volcanic_AddsColorPairs()
        {
            _colorizer.ApplyPreset(PlanetColorizer.PlanetType.Volcanic);
            var field = typeof(PlanetColorizer).GetField("m_heightColorPairs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pairs = field.GetValue(_colorizer) as List<PlanetColorizer.HeightColorPair>;
            Assert.IsNotNull(pairs);
            Assert.Greater(pairs.Count, 0);
        }

        [Test]
        public void ApplyPreset_Ocean_AddsColorPairs()
        {
            _colorizer.ApplyPreset(PlanetColorizer.PlanetType.Ocean);
            var field = typeof(PlanetColorizer).GetField("m_heightColorPairs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pairs = field.GetValue(_colorizer) as List<PlanetColorizer.HeightColorPair>;
            Assert.IsNotNull(pairs);
            Assert.Greater(pairs.Count, 0);
        }

        [Test]
        public void ApplyPreset_Jungle_AddsColorPairs()
        {
            _colorizer.ApplyPreset(PlanetColorizer.PlanetType.Jungle);
            var field = typeof(PlanetColorizer).GetField("m_heightColorPairs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pairs = field.GetValue(_colorizer) as List<PlanetColorizer.HeightColorPair>;
            Assert.IsNotNull(pairs);
            Assert.Greater(pairs.Count, 0);
        }
    }
}