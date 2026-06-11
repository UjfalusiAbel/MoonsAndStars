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
        [Range(1, 8)]
        public int numberOfLayers;
        public float baseRoughness;
        public float roughness;
        public float strength;
        public float persistance;
        public Vector3 center;
        public float minValue;
        public bool isEnabled;
        public bool useFirstLayerAsMask;
    }
}