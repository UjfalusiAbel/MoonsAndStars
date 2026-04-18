using System;
using System.Collections.Generic;
using System.Linq;
using MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects;
using Unity.VisualScripting;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.Models
{
    [Serializable]
    public class PlanetMeshData
    {
        [SerializeField] private float m_planetSize = 1f;
        [SerializeField] private List<NoiseFilter> m_noiseFilters;
        public float PlanetSize => m_planetSize;
        public QuadNode[] Roots { get; set; }
        public List<NoiseFilter> GetNoiseFilters => m_noiseFilters;
    }
}