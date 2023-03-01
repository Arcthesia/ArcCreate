using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class GlowingSpriteController : SpriteController, ISyncToSpeedController
    {
        private Color color;

        public override void UpdateColor(Color color)
        {
            this.color = color;
        }

        public void UpdateToSpeed(float speed, float glow)
        {
            float currentAlpha = Mathf.Lerp(0.75f, 1, glow);
            SpriteRenderer.color = color * new Color(1, 1, 1, currentAlpha);
        }
    }
}