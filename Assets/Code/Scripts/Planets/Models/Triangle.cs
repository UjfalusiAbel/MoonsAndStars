using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.Models
{
    public class Triangle
    {
        private int[] m_indices = new int[3];
        public int[] PointIndices => m_indices;
        public Triangle(int a, int b, int c)
        {
            m_indices[0] = a;
            m_indices[1] = b;
            m_indices[2] = c;
        }
    }
}