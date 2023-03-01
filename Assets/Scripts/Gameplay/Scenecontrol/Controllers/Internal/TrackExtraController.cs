using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class TrackExtraController : SpriteController
    {
#pragma warning disable
        public SpriteController DivideLine;
        public SpriteController Edge;
        public SpriteController CriticalLine;
#pragma warning restore
    }
}