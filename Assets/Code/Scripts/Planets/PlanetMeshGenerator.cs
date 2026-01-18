using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets
{
    public class PlanetMeshGenerator : MonoBehaviour
    {
        private PlanetMeshData m_meshData;
        private Mesh m_mesh;
        public List<MeshFilter> filters;
        public GameObject ball;

        private readonly static Triangle[] m_startTriangles = new Triangle[]
        {
            new Triangle(0,6,3), new Triangle(6,11,3), new Triangle(0,10,6), new Triangle(10,5,6),
            new Triangle(6,5,11), new Triangle(5,2,11), new Triangle(10,1,5), new Triangle(1,2,5),
            new Triangle(1,4,2), new Triangle(1,9,4), new Triangle(4,7,8), new Triangle(9,10,0),
            new Triangle(10,9,1), new Triangle(2,4,8), new Triangle(4,9,7), new Triangle(9,0,7),
            new Triangle(7,3,8), new Triangle(8,3,11), new Triangle(7,0,3), new Triangle(11,2,8),
        };
        private const float GOLDEN_RATIO = 1.618033988749f;

        private int[] GetTrianglesAsIntArray(List<Triangle> triangles)
        {
            List<int> intArray = new List<int>();
            foreach (var triangle in triangles)
            {
                intArray.AddRange(triangle.PointIndices);
            }
            return intArray.ToArray();
        }

        public static List<Vector3> GenerateInitialVertices(float meshSize)
        {
            float y = meshSize;
            float x = meshSize * GOLDEN_RATIO;
            Vector3 a = new Vector3(-x / 2f, y / 2f, 0f);
            Vector3 b = new Vector3(x / 2f, y / 2f, 0f);
            Vector3 c = new Vector3(x / 2f, -y / 2f, 0f);
            Vector3 d = new Vector3(-x / 2f, -y / 2f, 0f);

            List<Vector3> pointsPlaneA = new List<Vector3> { a, b, c, d };
            List<Vector3> pointsPlaneB = new List<Vector3>();
            List<Vector3> pointsPlaneC = new List<Vector3>();

            var rotationXY = Quaternion.Euler(90f, 90f, 0f);

            foreach (var point in pointsPlaneA)
            {
                pointsPlaneB.Add(rotationXY * point);
            }

            var rotationYZ = Quaternion.Euler(0f, 90f, 90f);

            foreach (var point in pointsPlaneA)
            {
                pointsPlaneC.Add(rotationYZ * point);
            }

            List<Vector3> vertices = new List<Vector3>();
            vertices.AddRange(pointsPlaneA);
            vertices.AddRange(pointsPlaneB);
            vertices.AddRange(pointsPlaneC);

            return vertices;
        }

        public void Start()
        {
            GenerateMesh();
        }

        public void GenerateMesh()
        {
            var initialVertices = GenerateInitialVertices(2f);
            for (int i = 0; i < initialVertices.Count; i++)
            {
                initialVertices[i].Normalize();
            }

            var vertices = GenerateInitialVertices(2f);
            var triangles = m_startTriangles.ToList();

            foreach (var triangle in m_startTriangles)
            {
                var triVertices = new Vector3[] { initialVertices[triangle.A], initialVertices[triangle.B], initialVertices[triangle.C] };
                var result = DivideTriangle(triangle, triVertices, vertices.Count - 1);
                vertices.AddRange(result.Item1);
                triangles.Remove(triangle);
                triangles.AddRange(result.Item2);
            }

            Debug.Log(JsonConvert.SerializeObject(triangles));


            Vector2[] uvs = new Vector2[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = GetTrianglesAsIntArray(triangles);
            mesh.uv = uvs;

            var index = 0;
            foreach (var vertex in vertices)
            {
                var instance = Instantiate(ball);
                instance.transform.position = vertex;
                var tester = instance.AddComponent<TesterData>();
                tester.number = index;
                index++;
            }

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            filters[0].mesh = mesh;
        }

        private Tuple<List<Vector3>, List<Triangle>> DivideTriangle(Triangle triangle, Vector3[] vectors, int lastTriangle)
        {
            List<Triangle> divisions = new List<Triangle>();

            var ab = Vector3.Slerp(vectors[0], vectors[1], 0.5f);
            var ac = Vector3.Slerp(vectors[0], vectors[2], 0.5f);
            var bc = Vector3.Slerp(vectors[1], vectors[2], 0.5f);
            List<Vector3> newPoints = new List<Vector3>() { ab, ac, bc };

            divisions.Add(new Triangle(triangle.A, lastTriangle + 1, lastTriangle + 2));
            divisions.Add(new Triangle(lastTriangle + 1, triangle.B, lastTriangle + 3));
            divisions.Add(new Triangle(lastTriangle + 2, lastTriangle + 3, triangle.C));
            divisions.Add(new Triangle(lastTriangle + 3, lastTriangle + 2, lastTriangle + 1));

            return new(newPoints, divisions);
        }
    }
}