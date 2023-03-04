using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Text controller for composer text. Mostly used to update objects internally by ArcCreate")]
    public class ComposerController : TextController
    {
        [SerializeField] private GameplayData gameplayData;

        [MoonSharpHidden]
        public override void Reset()
        {
            base.Reset();
            Text = TextChannelBuilder.Constant(gameplayData.Composer.Value);
        }
    }
}