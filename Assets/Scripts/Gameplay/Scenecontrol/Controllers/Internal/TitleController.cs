using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class TitleController : TextController
    {
        [SerializeField] private GameplayData gameplayData;

        public override void Reset()
        {
            base.Reset();
            Text = TextChannel.Constant(gameplayData.Title.Value);
        }
    }
}