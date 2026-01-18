using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models;
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

        public static int[] GetStarterTriangles
        {
            get
            {
                List<int> triangles = new List<int>();
                foreach (var triangle in m_startTriangles)
                {
                    triangles.AddRange(triangle.PointIndices);
                }
                return triangles.ToArray();
            }
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
            var vertices = GenerateInitialVertices(1f);
            Vector2[] uvs = new Vector2[vertices.Count];
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = GetStarterTriangles;
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


    }
}