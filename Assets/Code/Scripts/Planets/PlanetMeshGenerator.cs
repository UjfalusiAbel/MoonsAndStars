using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets
{
    public class PlanetMeshGenerator : MonoBehaviour
    {
        [SerializeField] private PlanetMeshData m_meshData;
        private MeshFilter[] m_filters;
        [SerializeField] private int m_resolution = 2;
        [SerializeField] private GameObject m_cameraObject;
        private float m_lodDistance = 2f;
        private Dictionary<long, int> m_cache = new Dictionary<long, int>();
        private readonly Vector3[] m_directions = { Vector3.up, Vector3.down, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        public float SetRecalculateDistance
        {
            set
            {
                if (m_lodDistance != value)
                {

                }
                m_lodDistance = value;
            }
        }

        public void Awake()
        {
            m_filters = GetComponentsInChildren<MeshFilter>();
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

        private Vector3 CubeToSphere(Vector3 p)
        {
            float x = p.x;
            float y = p.y;
            float z = p.z;

            float x2 = x * x;
            float y2 = y * y;
            float z2 = z * z;

            return new Vector3(
                x * Mathf.Sqrt(1f - (y2 + z2) / 2f + (y2 * z2) / 3f),
                y * Mathf.Sqrt(1f - (z2 + x2) / 2f + (z2 * x2) / 3f),
                z * Mathf.Sqrt(1f - (x2 + y2) / 2f + (x2 * y2) / 3f)
            );
        }

        private void GenerateFace(int index)
        {
            var a = m_directions[index];
            var b = new Vector3(a.y, a.z, a.x);
            var c = Vector3.Cross(b, a);

            var vertices = new List<Vector3>();
            var triangles = new List<Triangle>();

            for (int x = 0; x < m_resolution + 1; x++)
            {
                for (int y = 0; y < m_resolution + 1; y++)
                {
                    var percentX = x / (float)m_resolution;
                    var percentY = y / (float)m_resolution;
                    var offsetB = (percentX - 0.5f) * 2f;
                    var offsetC = (percentY - 0.5f) * 2f;

                    var vertexOnCube = a + offsetB * b + offsetC * c;
                    var vertexOnSphere = CubeToSphere(vertexOnCube);
                    vertexOnSphere *= m_meshData.PlanetSize;
                    vertices.Add(vertexOnSphere);

                    if (x < m_resolution && y < m_resolution)
                    {
                        int i = x * (m_resolution + 1) + y;
                        int bottomLeft = i;
                        var bottomRight = i + m_resolution + 1;
                        int topLeft = i + 1;
                        int topRight = i + m_resolution + 2;

                        var triangle1 = new Triangle(bottomLeft, topLeft, bottomRight);
                        var triangle2 = new Triangle(topLeft, topRight, bottomRight);
                        triangles.Add(triangle1);
                        triangles.Add(triangle2);
                    }
                }
            }

            m_meshData.MeshFaces[index].Vertices = vertices;
            m_meshData.MeshFaces[index].Triangles = triangles;
        }

        public void GenerateMesh()
        {
            for (int i = 0; i < m_filters.Length; i++)
            {
                if (m_filters[i].mesh != null)
                {
                    m_filters[i].mesh = new Mesh();
                }

                GenerateFace(i);

                ApplyChangesToMesh(i);
            }
        }

        private void ApplyChangesToMesh(int meshIndex)
        {

            Vector2[] uvs = new Vector2[m_meshData.MeshFaces[meshIndex].Vertices.Count];
            for (int i = 0; i < m_meshData.MeshFaces[meshIndex].Vertices.Count; i++)
            {
                uvs[i] = new Vector2(m_meshData.MeshFaces[meshIndex].Vertices[i].x, m_meshData.MeshFaces[meshIndex].Vertices[i].z);
            }

            if (m_filters[meshIndex].mesh == null)
            {
                m_filters[meshIndex].mesh = new Mesh();
            }

            m_filters[meshIndex].mesh.vertices = m_meshData.MeshFaces[meshIndex].Vertices.ToArray();
            m_filters[meshIndex].mesh.triangles = GetTrianglesAsIntArray(m_meshData.MeshFaces[meshIndex].Triangles);
            m_filters[meshIndex].mesh.uv = uvs;

            m_filters[meshIndex].mesh.RecalculateBounds();
            m_filters[meshIndex].mesh.RecalculateNormals();
        }


        private void GenerateCollider()
        {
            var collider = gameObject.AddComponent<MeshCollider>();
            collider.enabled = true;
            //collider.sharedMesh = m_filter.mesh;
            collider.convex = true;
            collider.isTrigger = false;
        }
    }
}