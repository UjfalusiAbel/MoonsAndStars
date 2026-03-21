using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.Models
{
    public class MeshFace
    {
        public List<Vector3> Vertices { get; set; }
        public List<Triangle> Triangles { get; set; }

        public MeshFace()
        {
            Vertices = new List<Vector3>();
            Triangles = new List<Triangle>();
        }
    }
}