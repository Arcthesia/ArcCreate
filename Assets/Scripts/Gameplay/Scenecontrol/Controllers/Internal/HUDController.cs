using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class HUDController : CanvasController
    {
#pragma warning disable
        public ImageController Pause;
        public InfoPanelController InfoPanel;
#pragma warning restore
    }
}