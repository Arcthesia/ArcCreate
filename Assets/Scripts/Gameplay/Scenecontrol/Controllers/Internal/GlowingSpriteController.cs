using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Sprite controller for glowing objects. Mostly used to update objects internally by ArcCreate")]
    public class GlowingSpriteController : SpriteController
    {
        [SerializeField] private GlowingSprite glowingSprite;

        [MoonSharpHidden]
        public override void UpdateColor(Color color)
        {
            glowingSprite.Color = color;
        }
    }
}