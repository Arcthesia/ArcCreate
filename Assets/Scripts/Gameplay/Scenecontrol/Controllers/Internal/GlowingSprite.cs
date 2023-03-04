using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public class GlowingSprite : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer sprite;

        public Color Color { get; set; } = Color.white;

        public void ApplyGlow(float alpha)
        {
            sprite.color = Color * new Color(1, 1, 1, alpha);
        }
    }
}