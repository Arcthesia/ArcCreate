using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Controller for the HUD canvas")]
    public class HUDController : CanvasController
    {
#pragma warning disable
        [SerializeField] private ImageController pause;

        [EmmyDoc("Gets the controller for the pause button image")]
        public ImageController Pause
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(pause);
                return pause;
            }
        }
        [SerializeField] private InfoPanelController infoPanel;
        [EmmyDoc("Gets the controller for the information panel")]
        public InfoPanelController InfoPanel
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(infoPanel);
                return infoPanel;
            }
        }
#pragma warning restore

        public override void UpdateRect(float w, float h, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            // Disable width and height changes to HUD
            RectTransform.anchorMin = anchorMin;
            RectTransform.anchorMax = anchorMax;
            RectTransform.pivot = pivot;
        }
    }
}