using UnityEngine;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public struct TouchInput
    {
        public int Id;
        public float Lane;
        public Vector3 ScreenPos;
        public Vector3 VerticalPos;
        public bool IsTap;
        public TouchPhase Phase;

        public TouchInput(Touch touch, Ray cameraRay)
        {
            Id = touch.fingerId;
            ScreenPos = new Vector3(touch.position.x, touch.position.y);
            IsTap = touch.phase == TouchPhase.Began;
            Phase = touch.phase;

            (int lane, float vx, float vy) = Projection.CastRayOntoPlayfield(cameraRay);
            VerticalPos = new Vector3(vx, vy);
            Lane = lane;
        }

        public TouchInput(int id, Vector3 screenPos, bool isTap, TouchPhase phase, Ray cameraRay)
        {
            Id = id;
            ScreenPos = screenPos;
            IsTap = isTap;
            Phase = phase;

            (int lane, float vx, float vy) = Projection.CastRayOntoPlayfield(cameraRay);
            VerticalPos = new Vector3(vx, vy);
            Lane = lane;
        }
    }
}