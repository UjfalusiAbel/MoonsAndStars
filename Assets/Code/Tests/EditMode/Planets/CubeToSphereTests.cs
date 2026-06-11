using NUnit.Framework;
using MoonsAndStars.Assets.Code.Scripts.Planets;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Tests.EditMode.Planets
{
    [TestFixture]
    public class CubeToSphereTests
    {
        private PlanetMeshGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            GameObject go = new GameObject();
            _generator = go.AddComponent<PlanetMeshGenerator>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_generator.gameObject);
        }

        [Test]
        public void CubeToSphere_PreservesDirectionSign()
        {
            var method = typeof(PlanetMeshGenerator).GetMethod("CubeToSphere",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Vector3 up = (Vector3)method.Invoke(_generator, new object[] { Vector3.up });
            Vector3 down = (Vector3)method.Invoke(_generator, new object[] { Vector3.down });

            Assert.Greater(up.y, 0);
            Assert.Less(down.y, 0);
        }

        [Test]
        public void CubeToSphere_ReturnsNormalizedVector()
        {
            var method = typeof(PlanetMeshGenerator).GetMethod("CubeToSphere",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Vector3 result = (Vector3)method.Invoke(_generator, new object[] { new Vector3(1, 1, 1) });
            Assert.AreEqual(1f, result.magnitude, 0.001f);
        }
    }
}