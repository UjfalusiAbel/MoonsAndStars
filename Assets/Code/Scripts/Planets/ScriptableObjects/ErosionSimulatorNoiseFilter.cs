using MoonsAndStars.Assets.Code.Scripts.Planets.Models.Noise;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Planets/NoiseFilters/Erosion Simulation Filter")]
    public class ErosionSimulationNoiseFilter : NoiseFilter
    {
        [Range(0f, 1f)]
        public float erosionStrength = 0.3f;

        [Range(0f, 1f)]
        public float sedimentationRate = 0.5f;

        public int erosionIterations = 3;

        public override float EvaluatePoint(Vector3 point, NoiseDetails details)
        {
            float height = 0f;
            float frequency = details.baseRoughness;
            float amplitude = 1f;
            float amplitudeSum = 0f;

            for (int i = 0; i < details.numberOfLayers; i++)
            {
                Vector3 scaledPoint = point * frequency;
                float value = m_noise.Evaluate(scaledPoint);

                height += value * amplitude;
                amplitudeSum += amplitude;

                frequency *= details.roughness;
                amplitude *= details.persistance;
            }

            height /= amplitudeSum;

            float slope = GetSlope(point, height);
            float erosion = 1f - (slope * erosionStrength);
            erosion = Mathf.Clamp01(erosion);

            float sediment = (1f - erosion) * sedimentationRate;

            height = height * erosion + sediment * 0.5f;

            return height * details.strength;
        }

        private float GetSlope(Vector3 point, float height)
        {
            float epsilon = 0.01f;

            Vector3 right = point + Vector3.right * epsilon;
            Vector3 left = point - Vector3.right * epsilon;
            Vector3 up = point + Vector3.up * epsilon;
            Vector3 down = point - Vector3.up * epsilon;
            Vector3 forward = point + Vector3.forward * epsilon;
            Vector3 back = point - Vector3.forward * epsilon;

            float dx = (GetHeightRaw(right) - GetHeightRaw(left)) / (2f * epsilon);
            float dy = (GetHeightRaw(up) - GetHeightRaw(down)) / (2f * epsilon);
            float dz = (GetHeightRaw(forward) - GetHeightRaw(back)) / (2f * epsilon);

            return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private float GetHeightRaw(Vector3 point)
        {
            float height = 0f;
            float frequency = 1f;
            float amplitude = 1f;
            float amplitudeSum = 0f;

            for (int i = 0; i < 3; i++)
            {
                Vector3 scaledPoint = point * frequency;
                float value = m_noise.Evaluate(scaledPoint);

                height += value * amplitude;
                amplitudeSum += amplitude;

                frequency *= 2f;
                amplitude *= 0.5f;
            }

            return height / amplitudeSum;
        }
    }
}