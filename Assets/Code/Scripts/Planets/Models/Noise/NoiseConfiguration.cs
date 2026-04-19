using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoonsAndStars.Assets.Code.Scripts.Planets.ScriptableObjects;

namespace MoonsAndStars.Assets.Code.Scripts.Planets.Models.Noise
{
    [Serializable]
    public class NoiseConfiguration
    {
        public NoiseFilter filter;
        public NoiseDetails details;
    }
}