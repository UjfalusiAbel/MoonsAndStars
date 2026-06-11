using System;
using System.Collections.Generic;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Player;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets
{
    public class PlanetMeshGenerator : MonoBehaviour
    {
        [SerializeField] private PlanetMeshData m_meshData;
        [SerializeField] private int m_resolution = 64;
        [SerializeField] private GameObject m_cameraObject;
        [SerializeField] private Material m_planetMaterial;
        [SerializeField] private int m_maxLevel = 8;
        [SerializeField] private bool m_isCube;
        [SerializeField] private bool m_isTerrainApplied;
        
        [Header("Frustum Culling")]
        [SerializeField] private FrustumCuller m_frustumCuller;
        [SerializeField] private bool m_enableFrustumCulling = true;
        [SerializeField] private float m_frustumPadding = 100f;
        
        private float m_lodDistance = 2f;
        private readonly Vector3[] m_directions = { Vector3.up, Vector3.down, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        private Dictionary<QuadNode, Bounds> m_nodeBoundsCache = new Dictionary<QuadNode, Bounds>();
        private float m_lastLodUpdate;
        private float m_lodUpdateInterval = 0.05f;
        private bool m_isGenerating = false;
        
        public PlanetMeshData GetMeshData => m_meshData;
        
        public float SetRecalculateDistance
        {
            set
            {
                if (m_lodDistance != value)
                {
                    m_lodDistance = value;
                }
            }
        }
        
        public void Start()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            
            GenerateMesh();
            
            if (m_frustumCuller == null)
            {
                m_frustumCuller = FindFirstObjectByType<FrustumCuller>();
            }
            
            if (m_cameraObject == null && Camera.main != null)
            {
                m_cameraObject = Camera.main.gameObject;
            }
        }
        
        private void Update()
        {
            if (m_isGenerating) return;
            if (m_meshData == null || m_meshData.Roots == null) return;
            
            if (Time.time - m_lastLodUpdate >= m_lodUpdateInterval)
            {
                UpdateLod();
                m_lastLodUpdate = Time.time;
            }
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
            var uvs = new List<Vector2>();
            
            int res = Mathf.Max(2, m_resolution >> node.GetLevel);
            res = Mathf.Min(res, 64);
            
            for (int x = 0; x < res + 1; x++)
            {
                for (int y = 0; y < res + 1; y++)
                {
                    var percentX = x / (float)res;
                    var percentY = y / (float)res;
                    
                    Vector2 uv = new Vector2(
                        Mathf.Lerp(node.GetMinCoords.x, node.GetMaxCoords.x, percentX),
                        Mathf.Lerp(node.GetMinCoords.y, node.GetMaxCoords.y, percentY)
                    );
                    
                    var offsetB = (uv.x - 0.5f) * 2f;
                    var offsetC = (uv.y - 0.5f) * 2f;
                    
                    var vertex = a + offsetB * b + offsetC * c;
                    
                    if (!m_isCube)
                    {
                        vertex = CubeToSphere(vertex);
                        
                        if (m_isTerrainApplied)
                        {
                            Vector3 direction = vertex.normalized;
                            float elevation = PlanetTerrainGenerator.Singleton.EvaluateHeight(vertex, m_meshData.GetNoiseConfigurations);
                            float radius = m_meshData.PlanetSize + elevation;
                            vertex = direction * radius;
                        }
                        else
                        {
                            vertex *= m_meshData.PlanetSize;
                        }
                    }
                    else
                    {
                        vertex *= m_meshData.PlanetSize;
                    }
                    
                    vertices.Add(vertex);
                    uvs.Add(uv);
                    
                    if (x < res && y < res)
                    {
                        int i = x * (res + 1) + y;
                        int bottomLeft = i;
                        var bottomRight = i + res + 1;
                        int topLeft = i + 1;
                        int topRight = i + res + 2;
                        
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
            mesh.uv = uvs.ToArray();
            
            Vector3[] normals = new Vector3[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                normals[i] = vertices[i].normalized;
            }
            mesh.normals = normals;
            
            mesh.RecalculateTangents();
            
            return mesh;
        }
        
        public void CleanupMesh()
        {
            if (m_meshData == null) return;
            
            if (m_meshData.Roots != null)
            {
                for (int i = 0; i < m_meshData.Roots.Length; i++)
                {
                    if (m_meshData.Roots[i] != null)
                    {
                        CleanupNodeRecursive(m_meshData.Roots[i]);
                    }
                }
                m_meshData.Roots = null;
            }
            
            m_nodeBoundsCache.Clear();
        }
        
        private void CleanupNodeRecursive(QuadNode node)
        {
            if (node == null) return;
            
            if (!node.IsLeaf && node.GetChildren != null)
            {
                foreach (var child in node.GetChildren)
                {
                    CleanupNodeRecursive(child);
                }
            }
            
            if (node.GetMeshFilter != null && node.GetMeshFilter.sharedMesh != null)
            {
                if (Application.isPlaying)
                    Destroy(node.GetMeshFilter.sharedMesh);
                else
                    DestroyImmediate(node.GetMeshFilter.sharedMesh);
                node.GetMeshFilter.sharedMesh = null;
            }
            
            if (node.GetMeshObject != null)
            {
                if (Application.isPlaying)
                    Destroy(node.GetMeshObject);
                else
                    DestroyImmediate(node.GetMeshObject);
            }
        }
        
        public void GenerateMesh()
        {
            if (m_meshData == null)
            {
                Debug.LogError("PlanetMeshData is null!");
                return;
            }
            
            m_isGenerating = true;
            
            CleanupMesh();
            
            m_nodeBoundsCache.Clear();
            m_meshData.Roots = new QuadNode[6];
            
            for (int i = 0; i < 6; i++)
            {
                var root = new QuadNode(Vector2.zero, Vector2.one, 0, transform, m_planetMaterial);
                var mesh = GenerateFace(i, root);
                root.GetMeshFilter.mesh = mesh;
                root.GetMeshCollider.sharedMesh = mesh;
                m_meshData.Roots[i] = root;
            }
            
            m_isGenerating = false;
            
            var colorizer = GetComponent<PlanetColorizer>();
            if (colorizer != null)
            {
                colorizer.ManualRecolor();
            }
        }
        
        public void UpdateLod()
        {
            if (m_isGenerating) return;
            if (m_meshData == null || m_meshData.Roots == null) return;
            if (m_cameraObject == null) return;
            
            for (int i = 0; i < 6; i++)
            {
                if (m_meshData.Roots[i] == null) continue;
                
                bool faceVisible = true;
                
                if (m_enableFrustumCulling && m_frustumCuller != null)
                {
                    faceVisible = IsFaceVisible(i);
                }
                
                if (!faceVisible)
                {
                    HideNodeAndChildren(i, m_meshData.Roots[i]);
                    continue;
                }
                
                UpdateLodRecursive(i, m_meshData.Roots[i]);
            }
        }
        
        private void UpdateLodRecursive(int faceIndex, QuadNode node)
        {
            if (node == null) return;
            
            bool isVisible = true;
            
            if (m_enableFrustumCulling && m_frustumCuller != null)
            {
                isVisible = IsNodeVisible(faceIndex, node);
            }
            
            if (!isVisible)
            {
                node.SetVisible(false);
                return;
            }
            
            node.SetVisible(true);
            
            Vector3 nodeCenter = GetNodeCenter(faceIndex, node);
            float dist = Vector3.Distance(m_cameraObject.transform.position, nodeCenter);
            float threshold = m_lodDistance / (node.GetLevel + 1);
            
            if (dist < threshold && node.GetLevel < m_maxLevel)
            {
                if (node.IsLeaf)
                {
                    SubdivideMesh(faceIndex, node);
                }
                
                if (!node.IsLeaf && node.GetChildren != null)
                {
                    foreach (var child in node.GetChildren)
                    {
                        UpdateLodRecursive(faceIndex, child);
                    }
                }
            }
            else
            {
                if (!node.IsLeaf)
                {
                    MergeMesh(faceIndex, node);
                }
            }
        }
        
        private Vector3 GetNodeCenter(int faceIndex, QuadNode node)
        {
            Vector2 centerUV = (node.GetMinCoords + node.GetMaxCoords) * 0.5f;
            
            var a = m_directions[faceIndex];
            var b = new Vector3(a.y, a.z, a.x);
            var c = Vector3.Cross(b, a);
            
            float offsetB = (centerUV.x - 0.5f) * 2f;
            float offsetC = (centerUV.y - 0.5f) * 2f;
            
            Vector3 cube = a + offsetB * b + offsetC * c;
            Vector3 sphere = CubeToSphere(cube) * m_meshData.PlanetSize;
            
            return sphere;
        }
        
        private bool IsNodeVisible(int faceIndex, QuadNode node)
        {
            if (!m_enableFrustumCulling || m_frustumCuller == null)
            {
                return true;
            }
            
            Bounds bounds = CalculateNodeBounds(faceIndex, node);
            bounds.Expand(m_frustumPadding);
            
            return m_frustumCuller.IsBoundsInFrustum(bounds);
        }
        
        private Bounds CalculateNodeBounds(int faceIndex, QuadNode node)
        {
            Vector3 center = GetNodeCenter(faceIndex, node);
            float size = (node.GetMaxCoords.x - node.GetMinCoords.x) * m_meshData.PlanetSize * Mathf.PI;
            
            return new Bounds(center, Vector3.one * size);
        }
        
        private bool IsFaceVisible(int faceIndex)
        {
            if (!m_enableFrustumCulling || m_frustumCuller == null)
            {
                return true;
            }
            
            Vector3 faceCenter = GetFaceCenter(faceIndex);
            float faceRadius = m_meshData.PlanetSize;
            
            return m_frustumCuller.IsSphereInFrustum(faceCenter, faceRadius);
        }
        
        private Vector3 GetFaceCenter(int faceIndex)
        {
            var a = m_directions[faceIndex];
            Vector3 cube = a.normalized;
            Vector3 sphere = CubeToSphere(cube) * m_meshData.PlanetSize;
            return sphere;
        }
        
        private void HideNodeAndChildren(int faceIndex, QuadNode node)
        {
            if (node == null) return;
            
            node.SetVisible(false);
            
            if (!node.IsLeaf && node.GetChildren != null)
            {
                foreach (var child in node.GetChildren)
                {
                    HideNodeAndChildren(faceIndex, child);
                }
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
            node.SetVisible(true);
            
            var colorizer = GetComponent<PlanetColorizer>();
            if (colorizer != null)
            {
                colorizer.ManualRecolor();
            }
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
            node.SetVisible(true);
            
            var colorizer = GetComponent<PlanetColorizer>();
            if (colorizer != null)
            {
                colorizer.ManualRecolor();
            }
        }
        
        private void OnDestroy()
        {
            CleanupMesh();
        }
        
        private void OnDisable()
        {
            CleanupMesh();
        }
    }
}