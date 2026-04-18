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
        [SerializeField] private int m_resolution = 2;
        [SerializeField] private GameObject m_cameraObject;
        [SerializeField] private Material m_planetMaterial;
        [SerializeField] private int m_maxLevel = 6;
        [SerializeField] private bool m_isCube;
        [SerializeField] private bool m_isTerrainApplied;
        private float m_lodDistance = 2f;
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

        public void Start()
        {
            GenerateMesh();
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

        private Mesh GenerateFace(int index, QuadNode node)
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

                    Vector2 uv = new Vector2(Mathf.Lerp(node.GetMinCoords.x, node.GetMaxCoords.x, percentX), Mathf.Lerp(node.GetMinCoords.y, node.GetMaxCoords.y, percentY));

                    var offsetB = (uv.x - 0.5f) * 2f;
                    var offsetC = (uv.y - 0.5f) * 2f;

                    var vertex = a + offsetB * b + offsetC * c;
                    if (!m_isCube)
                    {
                        vertex = CubeToSphere(vertex);
                    }

                    if(m_isTerrainApplied)
                    {
                        vertex = PlanetTerrainGenerator.Singleton.EvaluateSpherePoint(vertex, m_meshData.GetNoiseFilters);
                    }

                    vertex *= m_meshData.PlanetSize;
                    vertices.Add(vertex);

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

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = GetTrianglesAsIntArray(triangles);
            Vector3[] normals = new Vector3[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                normals[i] = vertices[i].normalized;
            }

            mesh.normals = normals;

            return mesh;
        }

        public void GenerateMesh()
        {
            m_meshData.Roots = new QuadNode[6];

            for (int i = 0; i < 6; i++)
            {
                var root = new QuadNode(Vector2.zero, Vector2.one, 0, transform, m_planetMaterial);

                var mesh = GenerateFace(i, root);
                root.GetMeshFilter.mesh = mesh;
                root.GetMeshCollider.sharedMesh = mesh;
                m_meshData.Roots[i] = root;
            }
        }


        public void UpdateLod()
        {
            for (int i = 0; i < 6; i++)
            {
                UpdateLodRecursive(i, m_meshData.Roots[i]);
            }
        }

        private void UpdateLodRecursive(int faceIndex, QuadNode node)
        {
            Vector2 centerUV = (node.GetMinCoords + node.GetMaxCoords) * 0.5f;

            var a = m_directions[faceIndex];
            var b = new Vector3(a.y, a.z, a.x);
            var c = Vector3.Cross(b, a);

            float offsetB = (centerUV.x - 0.5f) * 2f;
            float offsetC = (centerUV.y - 0.5f) * 2f;

            Vector3 cube = a + offsetB * b + offsetC * c;
            Vector3 sphere = CubeToSphere(cube) * m_meshData.PlanetSize;

            float dist = Vector3.Distance(m_cameraObject.transform.position, sphere);
            float threshold = m_lodDistance / (node.GetLevel + 1);

            if (dist < threshold && node.GetLevel < m_maxLevel)
            {
                SubdivideMesh(faceIndex, node);

                if (!node.IsLeaf)
                {
                    foreach (var child in node.GetChildren)
                    {
                        UpdateLodRecursive(faceIndex, child);
                    }
                }
            }
            else
            {
                MergeMesh(faceIndex, node);
            }
        }


        public void SubdivideMesh(int faceIndex, QuadNode node)
        {
            if (!node.IsLeaf)
            {
                return;
            }

            Vector2 min = node.GetMinCoords;
            Vector2 max = node.GetMaxCoords;
            Vector2 center = (min + max) / 2f;

            QuadNode[] divisions = new QuadNode[4];
            divisions[0] = new QuadNode(min, center, node.GetLevel + 1, node.GetMeshFilter.transform, m_planetMaterial);
            divisions[1] = new QuadNode(new Vector2(center.x, min.y), new Vector2(max.x, center.y), node.GetLevel + 1, node.GetMeshFilter.transform, m_planetMaterial);
            divisions[2] = new QuadNode(new Vector2(min.x, center.y), new Vector2(center.x, max.y), node.GetLevel + 1, node.GetMeshFilter.transform, m_planetMaterial);
            divisions[3] = new QuadNode(center, max, node.GetLevel + 1, node.GetMeshFilter.transform, m_planetMaterial);

            for (int i = 0; i < 4; i++)
            {
                var mesh = GenerateFace(faceIndex, divisions[i]);
                divisions[i].GetMeshFilter.mesh = mesh;
                divisions[i].GetMeshCollider.sharedMesh = mesh;
            }

            node.SetChildren(divisions);
        }

        public void MergeMesh(int faceIndex, QuadNode node)
        {
            if (node.IsLeaf)
            {
                return;
            }

            node.DestroyChildren();
            var mesh = GenerateFace(faceIndex, node);
            node.GetMeshFilter.mesh = mesh;
            node.GetMeshCollider.sharedMesh = mesh;
        }
    }
}