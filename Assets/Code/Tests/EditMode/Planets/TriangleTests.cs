using NUnit.Framework;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models;

namespace MoonsAndStars.Assets.Code.Tests.EditMode.Planets
{
    [TestFixture]
    public class TriangleTests
    {
        [Test]
        public void Triangle_Constructor_SetsIndices()
        {
            Triangle tri = new Triangle(0, 1, 2);
            Assert.AreEqual(0, tri.A);
            Assert.AreEqual(1, tri.B);
            Assert.AreEqual(2, tri.C);
        }

        [Test]
        public void PointIndices_ReturnsCorrectArray()
        {
            Triangle tri = new Triangle(5, 10, 15);
            int[] indices = tri.PointIndices;
            Assert.AreEqual(5, indices[0]);
            Assert.AreEqual(10, indices[1]);
            Assert.AreEqual(15, indices[2]);
        }

        [Test]
        public void Triangle_WithSameIndices_IsValid()
        {
            Triangle tri = new Triangle(0, 0, 0);
            Assert.AreEqual(0, tri.A);
            Assert.AreEqual(0, tri.B);
            Assert.AreEqual(0, tri.C);
        }
    }
}