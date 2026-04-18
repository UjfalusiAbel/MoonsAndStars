using System;
using System.Collections.Generic;
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

        public Vector3 EvaluateSpherePoint(Vector3 point, List<NoiseFilter> filters)
        {
            foreach(var filter in filters)
            {
                point *= 1+filter.EvaluatePoint(point);
            }

            return point;
        }
    }
}