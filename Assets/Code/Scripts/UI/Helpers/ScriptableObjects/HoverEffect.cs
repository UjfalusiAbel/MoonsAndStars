using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoonsAndStars.Assets.Code.Scripts.UI.Helpers.Interfaces;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.UI.Helpers.ScriptableObjects
{
    public abstract class HoverEffect : ScriptableObject, IHoverEffect
    {
        public abstract void OnHoverEnter(GameObject target);
        public abstract void OnHoverExit(GameObject target);
    }
}