using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.UI.Helpers.Interfaces
{
    public interface IHoverEffect
    {
        public void OnHoverEnter(GameObject target);
        public void OnHoverExit(GameObject target);
    }
}