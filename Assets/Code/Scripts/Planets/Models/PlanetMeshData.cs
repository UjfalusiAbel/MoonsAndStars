using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.Models
{
    [Serializable]
    public class PlanetMeshData
    {
        [SerializeField] private float m_planetSize = 1f;
        public float PlanetSize => m_planetSize;
        public List<Vector3> Vertices { get; set; }
        public List<Triangle> Triangles { get; set; }

        public PlanetMeshData()
        {
            Vertices = new List<Vector3>();
            Triangles = new List<Triangle>();
        }
    }
}