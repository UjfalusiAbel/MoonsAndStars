using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.UI.Helpers.Providers
{
    public class HoverSprite : MonoBehaviour
    {
        [Header("Sprite to swap out original with on hover")]
        [SerializeField] private Sprite _hoverChangeSprite;
        [SerializeField] private Sprite _originalSprite;
        public Sprite GetHoverChangeSprite => _hoverChangeSprite;
        public Sprite GetOriginalSprite => _originalSprite;
    }
}