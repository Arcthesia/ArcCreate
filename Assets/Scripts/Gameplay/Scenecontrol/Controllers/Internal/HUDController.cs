using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class HUDController : CanvasController
    {
#pragma warning disable
        [SerializeField] private ImageController pause;
        public ImageController Pause
        {
            get
            {
                Services.Scenecontrol.ReferencedControllers.Add(pause);
                return pause;
            }
        }
        [SerializeField] private InfoPanelController infoPanel;
        public InfoPanelController InfoPanel
        {
            get
            {
                Services.Scenecontrol.ReferencedControllers.Add(infoPanel);
                return infoPanel;
            }
        }
#pragma warning restore
    }
}