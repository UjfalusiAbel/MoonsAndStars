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
        [SerializeField] private PlanetMeshData m_meshData;
        private MeshFilter m_filter;
        [SerializeField] private int m_resolution = 2;
        [SerializeField] private GameObject m_cameraObject;
        private float m_lodDistance = 2f;
        public float SetRecalculateDistance
        {
            set
            {
                if (m_lodDistance != value)
                {
                    //CalculateLoD();
                }
                m_lodDistance = value;
            }
        }

        private readonly static Triangle[] m_startTriangles = new Triangle[]
        {
            new Triangle(0,6,3), new Triangle(6,11,3), new Triangle(0,10,6), new Triangle(10,5,6),
            new Triangle(6,5,11), new Triangle(5,2,11), new Triangle(10,1,5), new Triangle(1,2,5),
            new Triangle(1,4,2), new Triangle(1,9,4), new Triangle(4,7,8), new Triangle(9,10,0),
            new Triangle(10,9,1), new Triangle(2,4,8), new Triangle(4,9,7), new Triangle(9,0,7),
            new Triangle(7,3,8), new Triangle(8,3,11), new Triangle(7,0,3), new Triangle(11,2,8),
        };
        private const float GOLDEN_RATIO = 1.618033988749f;

        public void Awake()
        {
            m_filter = GetComponent<MeshFilter>();
        }

        public void Start()
        {
            GenerateMesh();
            GenerateCollider();
        }

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

        public void GenerateMesh()
        {
            var vertices = GenerateInitialVertices(m_meshData.PlanetSize);
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i].Normalize();
            }

            var triangles = m_startTriangles.ToList();
            var cache = new Dictionary<long, int>();
            var newTriangles = new List<Triangle>();

            for (int i = 0; i < m_resolution; i++)
            {
                foreach (var tri in triangles)
                {
                    DivideTriangle(tri, vertices, cache, newTriangles);
                }

                var temp = triangles;
                triangles = newTriangles;
                newTriangles = temp;
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

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            m_filter.mesh = mesh;
        }

        private void CalculateLoD(List<Triangle> triangles, List<Triangle> newTriangles, List<Vector3> vertices, Dictionary<long, int> cache, float admittedDistance)
        {
            var trianglesDivided = new HashSet<Triangle>();

            bool wasChanged = false;

            foreach (var tri in triangles)
            {
                if (Vector3.Distance(vertices[tri.A], m_cameraObject.transform.position) < admittedDistance)
                {
                    DivideTriangle(tri, vertices, cache, newTriangles);
                    trianglesDivided.Add(tri);
                    wasChanged = true;
                }
            }

            if (wasChanged)
            {

                triangles.RemoveAll(t => trianglesDivided.Contains(t));
                triangles.AddRange(newTriangles);
            }
        }

        private long GetPointKey(int a, int b)
        {
            return a < b ? ((long)a << 32) + b : ((long)b << 32) + a;
        }

        private int GetOrCreateMidpoint(int index1, int index2, List<Vector3> vertices, Dictionary<long, int> cache)
        {
            var key = GetPointKey(index1, index2);

            if (cache.ContainsKey(key))
            {
                return cache[key];
            }

            Vector3 midpoint = Vector3.Slerp(vertices[index1], vertices[index2], 0.5f);
            int newIndex = vertices.Count;
            vertices.Add(midpoint);
            cache.Add(key, newIndex);

            return newIndex;
        }

        private void DivideTriangle(Triangle triangle, List<Vector3> vertices, Dictionary<long, int> cache, List<Triangle> output)
        {
            var ab = GetOrCreateMidpoint(triangle.A, triangle.B, vertices, cache);
            var bc = GetOrCreateMidpoint(triangle.B, triangle.C, vertices, cache);
            var ca = GetOrCreateMidpoint(triangle.C, triangle.A, vertices, cache);

            output.Add(new Triangle(triangle.A, ab, ca));
            output.Add(new Triangle(triangle.B, bc, ab));
            output.Add(new Triangle(triangle.C, ca, bc));
            output.Add(new Triangle(ab, bc, ca));
        }

        private void GenerateCollider()
        {
            var collider = gameObject.AddComponent<MeshCollider>();
            collider.enabled = true;
            collider.sharedMesh = m_filter.mesh;
            collider.convex = true;
            collider.isTrigger = false;
        }
    }
}