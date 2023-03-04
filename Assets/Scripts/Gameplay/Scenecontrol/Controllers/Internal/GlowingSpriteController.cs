using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class GlowingSpriteController : SpriteController
    {
        [SerializeField] private GlowingSprite glowingSprite;
        private Color color;

        public override void UpdateColor(Color color)
        {
            glowingSprite.Color = color;
        }

        public void UpdateToSpeed(float speed, float glow)
        {
        }
    }
}