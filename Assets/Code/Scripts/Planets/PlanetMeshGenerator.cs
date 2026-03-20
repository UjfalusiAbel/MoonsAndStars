using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models;
using MoonsAndStars.Assets.Code.Scripts.Planets.Enums;
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
        [SerializeField] private PlanetBaseType m_planetBase;
        private float m_lodDistance = 2f;
        private Dictionary<long, int> m_cache = new Dictionary<long, int>();
        public float SetRecalculateDistance
        {
            set
            {
                if (m_lodDistance != value)
                {
                    CalculateLoD(m_meshData.Triangles, new List<Triangle>(), m_meshData.Vertices, m_lodDistance);
                    ApplyChangesToMesh();
                }
                m_lodDistance = value;
            }
        }

        private readonly Triangle[] m_icoSphereTriangles = new Triangle[]
        {
            new Triangle(0,6,3), new Triangle(6,11,3), new Triangle(0,10,6), new Triangle(10,5,6),
            new Triangle(6,5,11), new Triangle(5,2,11), new Triangle(10,1,5), new Triangle(1,2,5),
            new Triangle(1,4,2), new Triangle(1,9,4), new Triangle(4,7,8), new Triangle(9,10,0),
            new Triangle(10,9,1), new Triangle(2,4,8), new Triangle(4,9,7), new Triangle(9,0,7),
            new Triangle(7,3,8), new Triangle(8,3,11), new Triangle(7,0,3), new Triangle(11,2,8),
        };

        private readonly Triangle[] m_cubeSphereTriangles = new Triangle[]
        {
            
        };

        public Triangle[] GetStartTriangles
        {
            get
            {
                return m_planetBase == PlanetBaseType.Icosahedron ? m_icoSphereTriangles : m_cubeSphereTriangles;
            }
        }

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
            m_meshData.Vertices = GenerateInitialVertices(m_meshData.PlanetSize);
            for (int i = 0; i < m_meshData.Vertices.Count; i++)
            {
                m_meshData.Vertices[i] = m_meshData.Vertices[i].normalized;
            }

            m_meshData.Triangles = GetStartTriangles.ToList();
            var newTriangles = new List<Triangle>();

            for (int i = 0; i < m_resolution; i++)
            {
                newTriangles.Clear();
                m_cache.Clear();

                foreach (var tri in m_meshData.Triangles)
                {
                    DivideTriangle(tri, m_meshData.Vertices, newTriangles);
                }

                var temp = m_meshData.Triangles;
                m_meshData.Triangles = newTriangles;
                newTriangles = temp;
            }

            Debug.Log(JsonConvert.SerializeObject(m_meshData.Triangles));

            ApplyChangesToMesh();
        }

        private void ApplyChangesToMesh()
        {

            Vector2[] uvs = new Vector2[m_meshData.Vertices.Count];
            for (int i = 0; i < m_meshData.Vertices.Count; i++)
            {
                uvs[i] = new Vector2(m_meshData.Vertices[i].x, m_meshData.Vertices[i].z);
            }

            if (m_filter.mesh == null)
            {
                m_filter.mesh = new Mesh();
            }

            m_filter.mesh.vertices = m_meshData.Vertices.ToArray();
            m_filter.mesh.triangles = GetTrianglesAsIntArray(m_meshData.Triangles);
            m_filter.mesh.uv = uvs;

            m_filter.mesh.RecalculateBounds();
            m_filter.mesh.RecalculateNormals();
        }

        private void CalculateLoD(List<Triangle> triangles, List<Triangle> newTriangles, List<Vector3> vertices, float admittedDistance)
        {
            var trianglesDivided = new HashSet<Triangle>();

            bool wasChanged = false;
            m_cache.Clear();

            foreach (var tri in triangles)
            {
                var triCenter = (vertices[tri.A] + vertices[tri.B] + vertices[tri.C]) / 3f;
                if (Vector3.Distance(triCenter, m_cameraObject.transform.position) < admittedDistance)
                {
                    DivideTriangle(tri, vertices, newTriangles);
                    trianglesDivided.Add(tri);
                    wasChanged = true;
                }
            }

            if (wasChanged)
            {

                triangles.RemoveAll(t => trianglesDivided.Contains(t));
                triangles.AddRange(newTriangles);
                Debug.Log(JsonConvert.SerializeObject(newTriangles));
            }
        }

        private long GetPointKey(int a, int b)
        {
            return a < b ? ((long)a << 32) + b : ((long)b << 32) + a;
        }

        private int GetOrCreateMidpoint(int index1, int index2, List<Vector3> vertices)
        {
            var key = GetPointKey(index1, index2);

            if (m_cache.ContainsKey(key))
            {
                return m_cache[key];
            }

            Vector3 midpoint = Vector3.Slerp(vertices[index1], vertices[index2], 0.5f);
            int newIndex = vertices.Count;
            vertices.Add(midpoint);
            m_cache.Add(key, newIndex);

            return newIndex;
        }

        private void DivideTriangle(Triangle triangle, List<Vector3> vertices, List<Triangle> output)
        {
            var ab = GetOrCreateMidpoint(triangle.A, triangle.B, vertices);
            var bc = GetOrCreateMidpoint(triangle.B, triangle.C, vertices);
            var ca = GetOrCreateMidpoint(triangle.C, triangle.A, vertices);

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