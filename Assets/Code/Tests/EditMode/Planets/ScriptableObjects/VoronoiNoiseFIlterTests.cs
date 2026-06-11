using NUnit.Framework;
using MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models.Noise;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Tests.EditMode.Planets.ScriptableObjects
{
    [TestFixture]
    public class VoronoiNoiseFilterTests
    {
        private VoronoiNoiseFilter _filter;
        private NoiseDetails _details;

        [SetUp]
        public void SetUp()
        {
            _filter = ScriptableObject.CreateInstance<VoronoiNoiseFilter>();
            _details = new NoiseDetails
            {
                numberOfLayers = 1,
                baseRoughness = 1f,
                roughness = 1f,
                strength = 1f,
                persistance = 1f,
                center = Vector3.zero,
                minValue = 0f
            };
            _filter.cellSize = 1f;
            _filter.numPoints = 8;
            _filter.returnType = VoronoiNoiseFilter.VoronoiReturnType.F1;
        }

        [Test]
        public void EvaluatePoint_ReturnsValueInRange()
        {
            float result = _filter.EvaluatePoint(Vector3.zero, _details);
            Assert.IsTrue(result >= -1f && result <= 1f);
        }

        [Test]
        public void DifferentReturnTypes_ProduceDifferentValues()
        {
            _filter.returnType = VoronoiNoiseFilter.VoronoiReturnType.F1;
            float f1 = _filter.EvaluatePoint(Vector3.zero, _details);
            _filter.returnType = VoronoiNoiseFilter.VoronoiReturnType.F2;
            float f2 = _filter.EvaluatePoint(Vector3.zero, _details);
            Assert.AreNotEqual(f1, f2);
        }
        
        [Test]
        public void CellSize_AffectsOutput()
        {
            Vector3 samplePoint = new Vector3(1.5f, 2.3f, 0.7f);

            _filter.cellSize = 0.5f;
            float result1 = _filter.EvaluatePoint(samplePoint, _details);

            _filter.cellSize = 2f;
            float result2 = _filter.EvaluatePoint(samplePoint, _details);

            Assert.AreNotEqual(result1, result2, "Different cell sizes should produce different values");
        }
    }
}