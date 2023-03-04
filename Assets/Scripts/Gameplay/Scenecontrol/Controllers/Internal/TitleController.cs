using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Controller for the HUD's title text")]
    public class TitleController : TextController
    {
        [SerializeField] private GameplayData gameplayData;

        [MoonSharpHidden]
        public override void Reset()
        {
            base.Reset();
            Text = TextChannelBuilder.Constant(gameplayData.Title.Value);
        }
    }
}