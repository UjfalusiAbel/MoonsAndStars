using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.Models
{
    [Serializable]
    public class PlanetMeshData
    {
        [SerializeField]
        private MeshParameters m_meshParams;
        public List<Vector3> FinalPoints { get; set; }
        public List<int> FinalTriangles { get; set; }
    }
}