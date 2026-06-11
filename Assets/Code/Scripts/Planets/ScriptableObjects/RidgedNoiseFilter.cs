using MoonsAndStars.Assets.Code.Scripts.Planets.Models.Noise;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Planets/NoiseFilters/Ridged Noise Filter")]
    public class RidgedNoiseFilter : NoiseFilter
    {
        [Range(0f, 2f)]
        public float ridgeOffset = 1f;

        [Range(0f, 2f)]
        public float ridgeSharpness = 1f;

        public override float EvaluatePoint(Vector3 point, NoiseDetails details)
        {
            float noiseEvaluation = 0f;
            float frequency = details.baseRoughness;
            float amplitude = 1f;
            float amplitudeSum = 0f;
            float maxValue = 0f;

            for (int i = 0; i < details.numberOfLayers; i++)
            {
                Vector3 scaledPoint = point * frequency;
                float value = m_noise.Evaluate(scaledPoint);

                value = ridgeOffset - Mathf.Abs(value);
                value = Mathf.Pow(value, ridgeSharpness);

                noiseEvaluation += value * amplitude;
                amplitudeSum += amplitude;

                if (noiseEvaluation > maxValue) maxValue = noiseEvaluation;

                frequency *= details.roughness;
                amplitude *= details.persistance;
            }

            noiseEvaluation /= amplitudeSum;
            return noiseEvaluation * details.strength;
        }
    }
}