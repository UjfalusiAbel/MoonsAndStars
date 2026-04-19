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
                if(m_instance == null)
                {
                    m_instance = new PlanetTerrainGenerator();
                }

                return m_instance;
            }
        }

        public Vector3 EvaluateSpherePoint(Vector3 point, List<NoiseConfiguration> noiseConfigurations)
        {
            foreach(var configuration in noiseConfigurations)
            {
                point *= 1+configuration.filter.EvaluatePoint(point, configuration.details);
            }

            return point;
        }
    }
}