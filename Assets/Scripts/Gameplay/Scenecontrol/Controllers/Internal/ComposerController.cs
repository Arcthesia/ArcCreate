using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class ComposerController : TextController
    {
        [SerializeField] private GameplayData gameplayData;

        public override void Reset()
        {
            base.Reset();
            Text = TextChannel.Constant(gameplayData.Composer.Value);
        }
    }
}