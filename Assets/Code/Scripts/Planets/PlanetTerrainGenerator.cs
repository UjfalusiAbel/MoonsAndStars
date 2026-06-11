using System;
using System.Collections.Generic;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models.Noise;
using MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets
{
    public class PlanetTerrainGenerator
    {
        private static PlanetTerrainGenerator m_instance;
        public static PlanetTerrainGenerator Singleton
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new PlanetTerrainGenerator();
                }

                return m_instance;
            }
        }

        public float EvaluateHeight(Vector3 point, List<NoiseConfiguration> noiseConfigurations)
        {
            if (noiseConfigurations == null || noiseConfigurations.Count == 0)
            {
                return 0f;
            }

            float height = 0f;
            float firstLayerValue = 0f;

            Vector3 normalizedPoint = point.normalized;

            if (noiseConfigurations.Count > 0)
            {
                var firstConfig = noiseConfigurations[0];
                firstLayerValue = firstConfig.filter.EvaluatePoint(normalizedPoint, firstConfig.details);
                if (firstConfig.details.isEnabled)
                {
                    height = firstLayerValue;
                }
            }

            for (int i = 1; i < noiseConfigurations.Count; i++)
            {
                if (noiseConfigurations[i] != null && noiseConfigurations[i].filter != null && noiseConfigurations[i].details.isEnabled)
                {
                    float mask = (noiseConfigurations[i].details.useFirstLayerAsMask) ? firstLayerValue : 1f;
                    height += noiseConfigurations[i].filter.EvaluatePoint(normalizedPoint, noiseConfigurations[i].details) * mask;
                }
            }

            return height;
        }
    }
}