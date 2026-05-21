using System;
using MoonsAndStars.Assets.Code.Scripts.UI.Helpers.Providers;
using UnityEngine;
using UnityEngine.UI;

namespace MoonsAndStars.Assets.Code.Scripts.UI.Helpers.ScriptableObjects
{
    [CreateAssetMenu(menuName = "UI/Hover effects/Sprite change", fileName = "SpriteChangeHoverEffect")]
    public class SpriteChangeHoverEffect : HoverEffect
    {
        public override void OnHoverEnter(GameObject target)
        {
            if (target.TryGetComponent<Image>(out var image) && target.TryGetComponent<HoverSprite>(out var hoverSprite))
            {
                image.sprite = hoverSprite.GetHoverChangeSprite;
            }
            else
            {
                Debug.LogError("No image or hoversprite found!");
            }
        }

        public override void OnHoverExit(GameObject target)
        {
            if (target.TryGetComponent<Image>(out var image) && target.TryGetComponent<HoverSprite>(out var hoverSprite))
            {
                image.sprite = hoverSprite.GetOriginalSprite;
            }
            else
            {
                Debug.LogError("No image or hoversprite found!");
            }
        }
    }
}