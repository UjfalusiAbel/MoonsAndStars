using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models.Noise;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Planets/NoiseFilters/Simple noise filter")]
    public class SimpleNoiseFilter : NoiseFilter
    {
        public override float EvaluatePoint(Vector3 point, NoiseDetails details)
        {
            float noiseEvaluation = 0f;
            float frequency = details.baseRoughness;
            float amplitude = 1f;

            for (int i = 0; i < details.numberOfLayers; i++)
            {
                Vector3 scaledPoint = point * frequency + details.center;

                float value = m_noise.Evaluate(scaledPoint);

                noiseEvaluation += (value + 1) * 0.5f * amplitude;
                frequency *= details.roughness;
                amplitude *= details.persistance;
            }

            noiseEvaluation = Mathf.Max(0, noiseEvaluation - details.minValue);

            return noiseEvaluation * details.strength;
        }
    }
}