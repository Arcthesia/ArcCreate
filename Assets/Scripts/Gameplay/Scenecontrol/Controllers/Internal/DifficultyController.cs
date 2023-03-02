using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class DifficultyController : TextController
    {
        [SerializeField] private GameplayData gameplayData;

        public override void Reset()
        {
            base.Reset();
            Text = TextChannelBuilder.Constant(gameplayData.DifficultyName.Value);
        }
    }
}