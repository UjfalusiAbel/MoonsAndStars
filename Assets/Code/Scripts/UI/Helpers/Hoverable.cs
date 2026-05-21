using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoonsAndStars.Assets.Code.Scripts.UI.Helpers.ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoonsAndStars.Assets.Code.Scripts.UI.Helpers
{
    public class Hoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] List<HoverEffect> _hoverEffects;

        public void OnPointerEnter(PointerEventData eventData)
        {
            foreach(var effect in _hoverEffects)
            {
                effect.OnHoverEnter(gameObject);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            foreach(var effect in _hoverEffects)
            {
                effect.OnHoverExit(gameObject);
            }
        }
    }
}