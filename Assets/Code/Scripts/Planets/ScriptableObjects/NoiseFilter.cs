using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models.Noise;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects
{
    public abstract class NoiseFilter : ScriptableObject
    {
        protected Noise m_noise;
        public NoiseFilter()
        {
            m_noise = new Noise();
        }
        public abstract float EvaluatePoint(Vector3 point, NoiseDetails details);
    }
}