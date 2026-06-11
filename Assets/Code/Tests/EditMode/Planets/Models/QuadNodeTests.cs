using NUnit.Framework;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Tests.EditMode.Planets.Models
{

    [TestFixture]
    public class QuadNodeTests
    {
        [Test]
        public void QuadNode_Constructor_InitializesProperties()
        {
            Transform parent = new GameObject().transform;
            Material mat = new Material(Shader.Find("Standard"));
            QuadNode node = new QuadNode(Vector2.zero, Vector2.one, 0, parent, mat);

            Assert.AreEqual(Vector2.zero, node.GetMinCoords);
            Assert.AreEqual(Vector2.one, node.GetMaxCoords);
            Assert.AreEqual(0, node.GetLevel);
            Assert.IsTrue(node.IsLeaf);
            Assert.IsNotNull(node.GetMeshFilter);
            Assert.IsNotNull(node.GetMeshCollider);

            Object.DestroyImmediate(parent.gameObject);
        }

        [Test]
        public void SetChildren_MakesNodeNotLeaf()
        {
            Transform parent = new GameObject().transform;
            Material mat = new Material(Shader.Find("Standard"));
            QuadNode node = new QuadNode(Vector2.zero, Vector2.one, 0, parent, mat);

            QuadNode[] children = new QuadNode[0];
            node.SetChildren(children);

            Assert.IsFalse(node.IsLeaf);

            Object.DestroyImmediate(parent.gameObject);
        }

        [Test]
        public void SetVisible_ChangesVisibility()
        {
            Transform parent = new GameObject().transform;
            Material mat = new Material(Shader.Find("Standard"));
            QuadNode node = new QuadNode(Vector2.zero, Vector2.one, 0, parent, mat);

            node.SetVisible(false);
            Assert.IsFalse(node.IsVisible());

            node.SetVisible(true);
            Assert.IsTrue(node.IsVisible());

            Object.DestroyImmediate(parent.gameObject);
        }
    }

}