using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public struct TouchInput
    {
        public int Id;
        public int Lane;
        public float ScreenX;
        public float ScreenY;
        public float VerticalX;
        public float VerticalY;
        public bool IsTap;

        public TouchInput(Touch touch, Ray cameraRay)
        {
            Id = touch.touchId;
            ScreenX = touch.screenPosition.x;
            ScreenY = touch.screenPosition.y;
            IsTap = touch.began;

            (Lane, VerticalX, VerticalY) = Projection.CastRayOntoPlayfield(cameraRay);
        }
    }
}