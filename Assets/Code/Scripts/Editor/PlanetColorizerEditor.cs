using UnityEngine;
using UnityEditor;
using MoonsAndStars.Assets.Code.Scripts.Planets;

namespace MoonsAndStars.Assets.Code.Scripts.Editor
{
    [CustomEditor(typeof(PlanetColorizer))]
    public class PlanetColorizerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PlanetColorizer colorizer = (PlanetColorizer)target;

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Apply Colors Now", GUILayout.Height(30)))
            {
                colorizer.ManualRecolor();
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Earth Like"))
            {
                colorizer.SetPlanetType(PlanetColorizer.PlanetType.EarthLike);
                colorizer.ManualRecolor();
            }

            if (GUILayout.Button("Desert"))
            {
                colorizer.SetPlanetType(PlanetColorizer.PlanetType.Desert);
                colorizer.ManualRecolor();
            }

            if (GUILayout.Button("Ice"))
            {
                colorizer.SetPlanetType(PlanetColorizer.PlanetType.Ice);
                colorizer.ManualRecolor();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Volcanic"))
            {
                colorizer.SetPlanetType(PlanetColorizer.PlanetType.Volcanic);
                colorizer.ManualRecolor();
            }

            if (GUILayout.Button("Ocean"))
            {
                colorizer.SetPlanetType(PlanetColorizer.PlanetType.Ocean);
                colorizer.ManualRecolor();
            }

            if (GUILayout.Button("Jungle"))
            {
                colorizer.SetPlanetType(PlanetColorizer.PlanetType.Jungle);
                colorizer.ManualRecolor();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Randomize Colors", GUILayout.Height(30)))
            {
                colorizer.RandomizeColors();
            }

            EditorGUILayout.EndVertical();
        }
    }
}