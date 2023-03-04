using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Controller for the scene's camera")]
    public class CameraController : Controller, ICameraController, IPositionController
    {
        [EmmyDoc("Channel for the camera's field of view. Value of this channel is added to the camera's internal field of view value (Default is 0)")]
        public ValueChannel FieldOfView { get; set; }

        [EmmyDoc("Channel for the camera's tilt factor. Camera tilting rotation is multipled with the value of this channel (Default is 1)")]
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

        [MoonSharpHidden] public float DefaultFieldOfView => 0;

        [MoonSharpHidden] public Vector3 DefaultTranslation => Vector3.zero;

        [MoonSharpHidden] public Quaternion DefaultRotation => Quaternion.identity;

        [MoonSharpHidden] public Vector3 DefaultScale => Vector3.one;

        [MoonSharpHidden]
        public void UpdateCamera(float fieldOfView, float tiltFactor)
        {
            Services.Camera.SetPropertiesExternal(fieldOfView, tiltFactor);
        }

        [MoonSharpHidden]
        public void UpdatePosition(Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            Services.Camera.SetTransformExternal(translation, rotation);
        }
    }
}