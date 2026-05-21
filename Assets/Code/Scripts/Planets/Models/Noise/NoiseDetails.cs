using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.Models.Noise
{
    [Serializable]
    public class NoiseDetails
    {
        [Range(1, 6)]
        public int numberOfLayers;
        public float baseAmplitude;
        public float strength;
        public float scale;
        public float effectiveness;
    }
}