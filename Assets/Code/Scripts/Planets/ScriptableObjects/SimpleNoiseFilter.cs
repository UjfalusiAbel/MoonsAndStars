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
            float frequency = 1f;
            float amplitude = 1f;
            float amplitudeSum = 0f;

            for (int i = 0; i < details.numberOfLayers; i++)
            {
                Vector3 scaledPoint = point * frequency;

                float value = m_noise.Evaluate(scaledPoint);

                noiseEvaluation += value * amplitude;
                amplitudeSum += amplitude;

                frequency *= details.scale;
                amplitude *= details.effectiveness;
            }

            noiseEvaluation /= amplitudeSum;

            return noiseEvaluation * details.strength;
        }
    }
}