using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoonsAndStars.Assets.Code.Scripts.Planets.Models;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects
{
    public abstract class NoiseFilter : ScriptableObject
    {
        [SerializeField] protected NoiseDetails m_noiseDetails;

        public abstract float EvaluatePoint(Vector3 point);
    }
}