using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects
{
    [CreateAssetMenu(menuName ="Planets/NoiseFilters/Simple noise filter")]
    public class SimpleNoiseFilter : NoiseFilter
    {
        public override float EvaluatePoint(Vector3 point)
        {
            var scaledPoint = point * m_noiseDetails.scale;
            float noiseEvaluation = Mathf.PerlinNoise(scaledPoint.x, scaledPoint.y) * m_noiseDetails.strength;
            return noiseEvaluation;
        }
    }
}