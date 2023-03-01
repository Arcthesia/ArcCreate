using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class CameraController : Controller, ICameraController, IPositionController
    {
        public ValueChannel FieldOfView { get; set; }

        public ValueChannel TiltFactor { get; set; }

        public ValueChannel TranslationX { get; set; }

        public ValueChannel TranslationY { get; set; }

        public ValueChannel TranslationZ { get; set; }

        public ValueChannel RotationX { get; set; }

        public ValueChannel RotationY { get; set; }

        public ValueChannel RotationZ { get; set; }

        public ValueChannel ScaleX { get; set; }

        public ValueChannel ScaleY { get; set; }

        public ValueChannel ScaleZ { get; set; }

        public float DefaultFieldOfView => 0;

        public Vector3 DefaultTranslation => Vector3.zero;

        public Quaternion DefaultRotation => Quaternion.identity;

        public Vector3 DefaultScale => Vector3.one;

        public void UpdateCamera(float fieldOfView, float tiltFactor)
        {
            Services.Camera.SetPropertiesExternal(fieldOfView, tiltFactor);
        }

        public void UpdatePosition(Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            Services.Camera.SetTransformExternal(translation, rotation);
        }
    }
}