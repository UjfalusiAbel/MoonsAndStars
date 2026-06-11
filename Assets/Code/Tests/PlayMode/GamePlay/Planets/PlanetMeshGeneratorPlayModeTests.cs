using System.Collections;
using NUnit.Framework;
using MoonsAndStars.Assets.Code.Scripts.Planets;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models;
using UnityEngine;
using UnityEngine.TestTools;

namespace MoonsAndStars.Assets.Code.Tests.PlayMode.Planets
{
    [TestFixture]
    public class PlanetMeshGeneratorPlayModeTests
    {
        private GameObject _planetObject;
        private PlanetMeshGenerator _generator;
        private PlanetMeshData _meshData;

        [SetUp]
        public void SetUp()
        {
            _planetObject = new GameObject("TestPlanet");
            _generator = _planetObject.AddComponent<PlanetMeshGenerator>();
            
            // PlanetMeshData is a regular class, not ScriptableObject
            _meshData = new PlanetMeshData();
            var field = typeof(PlanetMeshGenerator).GetField("m_meshData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(_generator, _meshData);
            
            // Set required material
            var materialField = typeof(PlanetMeshGenerator).GetField("m_planetMaterial",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Material mat = new Material(Shader.Find("Standard"));
            materialField.SetValue(_generator, mat);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_planetObject);
        }

        [UnityTest]
        public IEnumerator GenerateMesh_CreatesRootNodes()
        {
            _generator.GenerateMesh();
            yield return null;
            
            Assert.IsNotNull(_generator.GetMeshData);
            Assert.IsNotNull(_generator.GetMeshData.Roots);
            Assert.AreEqual(6, _generator.GetMeshData.Roots.Length);
        }

        [UnityTest]
        public IEnumerator CleanupMesh_RemovesAllNodes()
        {
            _generator.GenerateMesh();
            yield return null;
            
            _generator.CleanupMesh();
            yield return null;
            
            Assert.IsNull(_generator.GetMeshData.Roots);
        }

        [UnityTest]
        public IEnumerator SubdivideMesh_CreatesChildren()
        {
            _generator.GenerateMesh();
            yield return null;
            
            var node = _generator.GetMeshData.Roots[0];
            int initialChildren = node.GetChildren != null ? node.GetChildren.Length : 0;
            Assert.AreEqual(0, initialChildren);
            
            _generator.SubdivideMesh(0, node);
            yield return null;
            
            Assert.IsNotNull(node.GetChildren);
            Assert.AreEqual(4, node.GetChildren.Length);
        }
    }
}