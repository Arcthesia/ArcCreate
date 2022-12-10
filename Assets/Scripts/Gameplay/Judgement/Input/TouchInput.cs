using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public struct TouchInput
    {
        public int Id;
        public int Lane;
        public Vector3 ScreenPos;
        public Vector3 VerticalPos;
        public bool IsTap;
        public UnityEngine.InputSystem.TouchPhase Phase;

        public TouchInput(Touch touch, Ray cameraRay)
        {
            Id = touch.touchId;
            ScreenPos = new Vector3(touch.screenPosition.x, touch.screenPosition.y);
            IsTap = touch.began;
            Phase = touch.phase;

            (int lane, float vx, float vy) = Projection.CastRayOntoPlayfield(cameraRay);
            VerticalPos = new Vector3(vx, vy);
            Lane = lane;
        }
    }
}