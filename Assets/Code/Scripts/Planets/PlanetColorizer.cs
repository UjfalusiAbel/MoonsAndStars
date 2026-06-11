using System;
using System.Collections.Generic;
using UnityEngine;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models;
using Random = UnityEngine.Random;

namespace MoonsAndStars.Assets.Code.Scripts.Planets
{
    [RequireComponent(typeof(PlanetMeshGenerator))]
    public class PlanetColorizer : MonoBehaviour
    {
        [Serializable]
        public class HeightColorPair
        {
            [Range(-2f, 5f)]
            public float height;
            public Color color;
            
            public HeightColorPair(float height, Color color)
            {
                this.height = height;
                this.color = color;
            }
        }
        
        [Header("Height-Color Pairs (sorted by height)")]
        [SerializeField] private List<HeightColorPair> m_heightColorPairs = new List<HeightColorPair>();
        
        [Header("Planet Type Presets")]
        [SerializeField] private PlanetType m_planetType = PlanetType.Random;
        
        [Header("Random Generation Settings")]
        [SerializeField] private bool m_generateRandomOnStart = true;
        [SerializeField] private int m_randomSeed = 0;
        [SerializeField] private float m_colorVariation = 0.3f;
        [SerializeField] private float m_heightVariation = 0.5f;
        
        [Header("Settings")]
        [SerializeField] private bool m_autoColor = true;
        [SerializeField] private bool m_recolorOnLODChange = true;
        
        private PlanetMeshGenerator m_planetGenerator;
        private float m_lastLODTime;
        private int m_lastMeshCount = -1;
        
        public enum PlanetType
        {
            Random,
            EarthLike,
            Desert,
            Ice,
            Volcanic,
            Ocean,
            Jungle,
        }
        
        private void Start()
        {
            if (m_generateRandomOnStart)
            {
                GenerateRandomColorScheme();
            }
            
            if (m_autoColor)
            {
                Invoke(nameof(ColorAllMeshes), 0.2f);
            }
        }
        
        private void Update()
        {
            if (!m_recolorOnLODChange) return;
            if (m_planetGenerator == null) m_planetGenerator = GetComponent<PlanetMeshGenerator>();
            if (m_planetGenerator == null || m_planetGenerator.GetMeshData?.Roots == null) return;
            
            int currentMeshCount = CountTotalMeshes();
            
            if (currentMeshCount != m_lastMeshCount)
            {
                m_lastMeshCount = currentMeshCount;
                ColorAllMeshes();
            }
        }
        
        private int CountTotalMeshes()
        {
            if (m_planetGenerator?.GetMeshData?.Roots == null) return 0;
            
            int count = 0;
            foreach (var root in m_planetGenerator.GetMeshData.Roots)
            {
                if (root != null)
                    count += CountNodesRecursive(root);
            }
            return count;
        }
        
        private int CountNodesRecursive(QuadNode node)
        {
            if (node == null) return 0;
            
            int count = (node.GetMeshFilter?.mesh != null) ? 1 : 0;
            
            if (!node.IsLeaf && node.GetChildren != null)
            {
                foreach (var child in node.GetChildren)
                {
                    count += CountNodesRecursive(child);
                }
            }
            
            return count;
        }
        
        public void GenerateRandomColorScheme()
        {
            if (m_planetType == PlanetType.Random)
            {
                m_planetType = (PlanetType)Random.Range(1, Enum.GetValues(typeof(PlanetType)).Length);
            }
            
            ApplyPreset(m_planetType);
            
            if (m_randomSeed != 0)
            {
                Random.InitState(m_randomSeed);
            }
            
            for (int i = 0; i < m_heightColorPairs.Count; i++)
            {
                var pair = m_heightColorPairs[i];
                pair.color = new Color(
                    Mathf.Clamp01(pair.color.r + Random.Range(-m_colorVariation, m_colorVariation)),
                    Mathf.Clamp01(pair.color.g + Random.Range(-m_colorVariation, m_colorVariation)),
                    Mathf.Clamp01(pair.color.b + Random.Range(-m_colorVariation, m_colorVariation))
                );
                
                pair.height += Random.Range(-m_heightVariation, m_heightVariation);
                pair.height = Mathf.Clamp(pair.height, -2f, 5f);
                m_heightColorPairs[i] = pair;
            }
            
            SortPairsByHeight();
            
            Debug.Log($"Generated random color scheme for planet type: {m_planetType}");
        }
        
        public void ApplyPreset(PlanetType type)
        {
            m_heightColorPairs.Clear();
            
            switch (type)
            {
                case PlanetType.EarthLike:
                    m_heightColorPairs.Add(new HeightColorPair(-0.8f, new Color(0.02f, 0.05f, 0.15f)));   // Deep ocean
                    m_heightColorPairs.Add(new HeightColorPair(-0.3f, new Color(0.1f, 0.2f, 0.4f)));     // Shallow water
                    m_heightColorPairs.Add(new HeightColorPair(0f, new Color(0.76f, 0.7f, 0.5f)));       // Sand/beach
                    m_heightColorPairs.Add(new HeightColorPair(0.3f, new Color(0.2f, 0.55f, 0.15f)));    // Grass
                    m_heightColorPairs.Add(new HeightColorPair(0.7f, new Color(0.1f, 0.35f, 0.05f)));    // Forest
                    m_heightColorPairs.Add(new HeightColorPair(1.2f, new Color(0.55f, 0.45f, 0.35f)));   // Mountain
                    m_heightColorPairs.Add(new HeightColorPair(1.8f, new Color(0.95f, 0.95f, 0.95f)));   // Snow
                    break;
                    
                case PlanetType.Desert:
                    m_heightColorPairs.Add(new HeightColorPair(-0.5f, new Color(0.3f, 0.25f, 0.2f)));
                    m_heightColorPairs.Add(new HeightColorPair(0f, new Color(0.6f, 0.5f, 0.3f)));
                    m_heightColorPairs.Add(new HeightColorPair(0.4f, new Color(0.76f, 0.65f, 0.4f)));
                    m_heightColorPairs.Add(new HeightColorPair(0.8f, new Color(0.8f, 0.55f, 0.35f)));
                    m_heightColorPairs.Add(new HeightColorPair(1.3f, new Color(0.6f, 0.4f, 0.25f)));
                    m_heightColorPairs.Add(new HeightColorPair(2f, new Color(0.85f, 0.75f, 0.6f)));
                    break;
                    
                case PlanetType.Ice:
                    m_heightColorPairs.Add(new HeightColorPair(-0.8f, new Color(0.1f, 0.15f, 0.25f)));
                    m_heightColorPairs.Add(new HeightColorPair(-0.2f, new Color(0.3f, 0.4f, 0.5f)));
                    m_heightColorPairs.Add(new HeightColorPair(0.2f, new Color(0.7f, 0.75f, 0.8f)));
                    m_heightColorPairs.Add(new HeightColorPair(0.6f, new Color(0.85f, 0.85f, 0.9f)));
                    m_heightColorPairs.Add(new HeightColorPair(1f, new Color(0.95f, 0.95f, 1f)));
                    m_heightColorPairs.Add(new HeightColorPair(1.8f, new Color(1f, 1f, 1f)));
                    break;
                    
                case PlanetType.Volcanic:
                    m_heightColorPairs.Add(new HeightColorPair(-0.6f, new Color(0.1f, 0.05f, 0.1f)));
                    m_heightColorPairs.Add(new HeightColorPair(0f, new Color(0.2f, 0.1f, 0.15f)));
                    m_heightColorPairs.Add(new HeightColorPair(0.4f, new Color(0.4f, 0.2f, 0.15f)));
                    m_heightColorPairs.Add(new HeightColorPair(0.9f, new Color(0.6f, 0.25f, 0.15f)));
                    m_heightColorPairs.Add(new HeightColorPair(1.5f, new Color(0.8f, 0.3f, 0.15f)));
                    m_heightColorPairs.Add(new HeightColorPair(2.2f, new Color(1f, 0.4f, 0.1f)));
                    break;
                    
                case PlanetType.Ocean:
                    m_heightColorPairs.Add(new HeightColorPair(-1.2f, new Color(0.01f, 0.03f, 0.1f)));
                    m_heightColorPairs.Add(new HeightColorPair(-0.5f, new Color(0.05f, 0.1f, 0.2f)));
                    m_heightColorPairs.Add(new HeightColorPair(-0.1f, new Color(0.1f, 0.2f, 0.35f)));
                    m_heightColorPairs.Add(new HeightColorPair(0.2f, new Color(0.2f, 0.35f, 0.45f)));
                    m_heightColorPairs.Add(new HeightColorPair(0.6f, new Color(0.3f, 0.45f, 0.5f)));
                    m_heightColorPairs.Add(new HeightColorPair(1.2f, new Color(0.4f, 0.55f, 0.6f)));
                    break;
                    
                case PlanetType.Jungle:
                    m_heightColorPairs.Add(new HeightColorPair(-0.6f, new Color(0.05f, 0.08f, 0.12f)));
                    m_heightColorPairs.Add(new HeightColorPair(0f, new Color(0.15f, 0.25f, 0.1f)));
                    m_heightColorPairs.Add(new HeightColorPair(0.3f, new Color(0.1f, 0.4f, 0.1f)));
                    m_heightColorPairs.Add(new HeightColorPair(0.7f, new Color(0.08f, 0.35f, 0.08f)));
                    m_heightColorPairs.Add(new HeightColorPair(1.1f, new Color(0.3f, 0.2f, 0.1f)));
                    m_heightColorPairs.Add(new HeightColorPair(1.8f, new Color(0.8f, 0.7f, 0.5f)));
                    break;
            }
            
            SortPairsByHeight();
        }
        
        private void SortPairsByHeight()
        {
            m_heightColorPairs.Sort((a, b) => a.height.CompareTo(b.height));
        }
        
        public void ColorAllMeshes()
        {
            m_planetGenerator = GetComponent<PlanetMeshGenerator>();
            
            if (m_planetGenerator == null || m_planetGenerator.GetMeshData == null)
            {
                Debug.LogError("PlanetMeshGenerator or MeshData is null!");
                return;
            }
            
            if (m_planetGenerator.GetMeshData.Roots == null)
            {
                Debug.LogError("No mesh roots! Generate mesh first.");
                return;
            }
            
            if (m_heightColorPairs == null || m_heightColorPairs.Count == 0)
            {
                Debug.LogError("No height-color pairs defined!");
                return;
            }
            
            float baseRadius = m_planetGenerator.GetMeshData.PlanetSize;
            
            int coloredCount = 0;
            
            for (int i = 0; i < m_planetGenerator.GetMeshData.Roots.Length; i++)
            {
                var root = m_planetGenerator.GetMeshData.Roots[i];
                if (root != null)
                {
                    coloredCount += ColorNodeRecursive(root, baseRadius);
                }
            }
            
            Debug.Log($"Colored {coloredCount} meshes total");
        }
        
        private int ColorNodeRecursive(QuadNode node, float baseRadius)
        {
            if (node == null) return 0;
            
            int count = 0;
            
            if (node.GetMeshFilter != null && node.GetMeshFilter.mesh != null)
            {
                ColorMesh(node.GetMeshFilter.mesh, baseRadius);
                count++;
            }
            
            if (!node.IsLeaf && node.GetChildren != null)
            {
                foreach (var child in node.GetChildren)
                {
                    count += ColorNodeRecursive(child, baseRadius);
                }
            }
            
            return count;
        }
        
        private void ColorMesh(Mesh mesh, float baseRadius)
        {
            if (mesh == null) return;
            
            Vector3[] vertices = mesh.vertices;
            Color[] colors = new Color[vertices.Length];
            
            for (int i = 0; i < vertices.Length; i++)
            {
                float height = vertices[i].magnitude - baseRadius;
                colors[i] = GetColorFromPairs(height);
            }
            
            mesh.colors = colors;
            mesh.UploadMeshData(false);
        }
        
        private Color GetColorFromPairs(float height)
        {
            if (m_heightColorPairs == null || m_heightColorPairs.Count == 0)
            {
                return Color.white;
            }
            
            if (height <= m_heightColorPairs[0].height)
            {
                return m_heightColorPairs[0].color;
            }
            
            if (height >= m_heightColorPairs[m_heightColorPairs.Count - 1].height)
            {
                return m_heightColorPairs[m_heightColorPairs.Count - 1].color;
            }
            
            for (int i = 0; i < m_heightColorPairs.Count - 1; i++)
            {
                if (height >= m_heightColorPairs[i].height && height <= m_heightColorPairs[i + 1].height)
                {
                    float t = Mathf.InverseLerp(m_heightColorPairs[i].height, m_heightColorPairs[i + 1].height, height);
                    return Color.Lerp(m_heightColorPairs[i].color, m_heightColorPairs[i + 1].color, t);
                }
            }
            
            return m_heightColorPairs[m_heightColorPairs.Count - 1].color;
        }
        
        public void ManualRecolor()
        {
            ColorAllMeshes();
        }
        
        public void SetPlanetType(PlanetType type)
        {
            m_planetType = type;
            ApplyPreset(type);
            if (Application.isPlaying)
            {
                ColorAllMeshes();
            }
        }
        
        public void RandomizeColors()
        {
            GenerateRandomColorScheme();
            if (Application.isPlaying)
            {
                ColorAllMeshes();
            }
        }
        
        private void OnValidate()
        {
            if (m_heightColorPairs != null)
            {
                SortPairsByHeight();
            }
            
            if (Application.isPlaying && m_autoColor)
            {
                Invoke(nameof(ColorAllMeshes), 0.1f);
            }
        }
    }
}