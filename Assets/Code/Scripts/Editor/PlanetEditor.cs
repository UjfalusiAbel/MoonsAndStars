using UnityEngine;
using UnityEditor;
using MoonsAndStars.Assets.Code.Scripts.Planets;
using System.Collections.Generic;

namespace MoonsAndStars.Assets.Code.Scripts.Editor
{
    [CustomEditor(typeof(PlanetMeshGenerator))]
    public class PlanetEditor : UnityEditor.Editor
    {
        private PlanetMeshGenerator m_planet;
        private bool m_autoUpdate = true;
        private bool m_showAdvanced = false;
        
        private void OnEnable()
        {
            m_planet = (PlanetMeshGenerator)target;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space(10);
            
            DrawHeader();
            
            EditorGUILayout.Space(5);
            
            DrawTerrainSettings();
            
            EditorGUILayout.Space(5);
            
            DrawFrustumSettings();
            
            EditorGUILayout.Space(10);
            
            DrawActionButtons();
            
            EditorGUILayout.Space(5);
            
            DrawStatus();
            
            serializedObject.ApplyModifiedProperties();
            
            if (m_autoUpdate && GUI.changed)
            {
                RegeneratePlanet();
            }
        }
        
        private new void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 14;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            
            EditorGUILayout.LabelField("Planet Mesh Generator", titleStyle);
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = m_autoUpdate ? Color.green : Color.gray;
            if (GUILayout.Button(m_autoUpdate ? "Auto-Update: ON" : "Auto-Update: OFF", GUILayout.Height(25)))
            {
                m_autoUpdate = !m_autoUpdate;
            }
            
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Force Regenerate", GUILayout.Height(25)))
            {
                RegeneratePlanet();
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTerrainSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Terrain Settings", EditorStyles.boldLabel);
            
            SerializedProperty meshData = serializedObject.FindProperty("m_meshData");
            SerializedProperty resolution = serializedObject.FindProperty("m_resolution");
            SerializedProperty cameraObject = serializedObject.FindProperty("m_cameraObject");
            SerializedProperty planetMaterial = serializedObject.FindProperty("m_planetMaterial");
            SerializedProperty maxLevel = serializedObject.FindProperty("m_maxLevel");
            SerializedProperty isCube = serializedObject.FindProperty("m_isCube");
            SerializedProperty isTerrainApplied = serializedObject.FindProperty("m_isTerrainApplied");
            
            if (meshData != null) EditorGUILayout.PropertyField(meshData, new GUIContent("Planet Mesh Data"));
            if (resolution != null) EditorGUILayout.PropertyField(resolution, new GUIContent("Resolution"));
            if (cameraObject != null) EditorGUILayout.PropertyField(cameraObject, new GUIContent("Camera Object"));
            if (planetMaterial != null) EditorGUILayout.PropertyField(planetMaterial, new GUIContent("Planet Material"));
            if (maxLevel != null) EditorGUILayout.PropertyField(maxLevel, new GUIContent("Max LOD Level"));
            
            EditorGUILayout.BeginHorizontal();
            if (isCube != null) EditorGUILayout.PropertyField(isCube, new GUIContent("Is Cube"));
            if (isTerrainApplied != null) EditorGUILayout.PropertyField(isTerrainApplied, new GUIContent("Apply Terrain"));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawFrustumSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Frustum Culling", EditorStyles.boldLabel);
            
            SerializedProperty frustumCuller = serializedObject.FindProperty("m_frustumCuller");
            SerializedProperty enableFrustum = serializedObject.FindProperty("m_enableFrustumCulling");
            SerializedProperty frustumPadding = serializedObject.FindProperty("m_frustumPadding");
            
            if (frustumCuller != null) EditorGUILayout.PropertyField(frustumCuller, new GUIContent("Frustum Culler"));
            if (enableFrustum != null) EditorGUILayout.PropertyField(enableFrustum, new GUIContent("Enable Frustum Culling"));
            if (frustumPadding != null) EditorGUILayout.PropertyField(frustumPadding, new GUIContent("Frustum Padding"));
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawActionButtons()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Generate Mesh", GUILayout.Height(30)))
            {
                RegeneratePlanet();
            }
            
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Clear Mesh", GUILayout.Height(30)))
            {
                ClearPlanetMesh();
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.blue;
            if (GUILayout.Button("Center Camera", GUILayout.Height(25)))
            {
                CenterCameraOnPlanet();
            }
            
            GUI.backgroundColor = Color.magenta;
            if (GUILayout.Button("Export Mesh", GUILayout.Height(25)))
            {
                ExportPlanetMesh();
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawStatus()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Auto-Update:", m_autoUpdate ? "Enabled" : "Disabled");
            
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Mode:", "🎮 Play Mode");
            }
            else
            {
                EditorGUILayout.LabelField("Mode:", "Edit Mode");
            }
            
            if (m_planet.GetMeshData != null && m_planet.GetMeshData.Roots != null)
            {
                int visibleCount = 0;
                foreach (var root in m_planet.GetMeshData.Roots)
                {
                    if (root != null && root.IsVisible()) visibleCount++;
                }
                EditorGUILayout.LabelField("Visible Faces:", visibleCount.ToString());
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void RegeneratePlanet()
        {
            if (EditorApplication.isPlaying)
            {
                m_planet.GenerateMesh();
                Debug.Log("[Planet Editor] Planet regenerated in Play Mode");
            }
            else
            {
                Undo.RecordObject(m_planet, "Regenerate Planet");
                m_planet.GenerateMesh();
                EditorUtility.SetDirty(m_planet);
                Debug.Log("[Planet Editor] Planet regenerated");
            }
        }
        
        private void ClearPlanetMesh()
        {
            if (m_planet.GetMeshData != null && m_planet.GetMeshData.Roots != null)
            {
                for (int i = 0; i < m_planet.GetMeshData.Roots.Length; i++)
                {
                    if (m_planet.GetMeshData.Roots[i] != null)
                    {
                        var meshFilter = m_planet.GetMeshData.Roots[i].GetMeshFilter;
                        if (meshFilter != null && meshFilter.sharedMesh != null)
                        {
                            DestroyImmediate(meshFilter.sharedMesh);
                            meshFilter.sharedMesh = null;
                        }
                    }
                }
                
                m_planet.GetMeshData.Roots = null;
                Debug.Log("[Planet Editor] Mesh cleared");
            }
        }
        
        private void CenterCameraOnPlanet()
        {
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.LookAt(m_planet.transform.position, Quaternion.identity, 50f);
                Debug.Log("[Planet Editor] Camera centered on planet");
            }
            else
            {
                Debug.LogWarning("[Planet Editor] No active Scene View found");
            }
        }
        
        private void ExportPlanetMesh()
        {
            if (m_planet.GetMeshData == null || m_planet.GetMeshData.Roots == null)
            {
                Debug.LogWarning("[Planet Editor] No mesh to export");
                return;
            }
            
            string path = EditorUtility.SaveFilePanel("Export Planet Mesh", "Assets", "PlanetMesh", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = FileUtil.GetProjectRelativePath(path);
                
                Mesh combinedMesh = new Mesh();
                List<Mesh> allMeshes = new List<Mesh>();
                
                foreach (var root in m_planet.GetMeshData.Roots)
                {
                    if (root != null && root.GetMeshFilter != null && root.GetMeshFilter.sharedMesh != null)
                    {
                        allMeshes.Add(root.GetMeshFilter.sharedMesh);
                    }
                }
                
                if (allMeshes.Count > 0)
                {
                    CombineInstance[] combine = new CombineInstance[allMeshes.Count];
                    for (int i = 0; i < allMeshes.Count; i++)
                    {
                        combine[i].mesh = allMeshes[i];
                        combine[i].transform = Matrix4x4.identity;
                    }
                    combinedMesh.CombineMeshes(combine);
                    
                    AssetDatabase.CreateAsset(combinedMesh, path);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"[Planet Editor] Mesh exported to: {path}");
                }
            }
        }
        
        private void OnSceneGUI()
        {
            if (m_planet == null) return;
            
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10, 10, 200, 100), EditorStyles.helpBox);
            GUILayout.Label("Planet Editor Controls", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Regenerate"))
            {
                RegeneratePlanet();
            }
            
            if (GUILayout.Button("Center View"))
            {
                CenterCameraOnPlanet();
            }
            
            GUILayout.EndArea();
            Handles.EndGUI();
        }
    }
}