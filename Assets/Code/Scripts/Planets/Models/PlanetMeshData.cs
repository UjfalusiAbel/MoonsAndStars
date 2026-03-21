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
        public MeshFace[] MeshFaces { get; set; }

        public PlanetMeshData()
        {
            MeshFaces = new MeshFace[6];
            for (int i = 0; i < 6; i++)
            {
                MeshFaces[i] = new MeshFace();
            }
        }
    }
}