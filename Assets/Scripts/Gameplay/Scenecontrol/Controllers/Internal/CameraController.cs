using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Controller for the scene's camera")]
    public class CameraController : Controller, ICameraController, IPositionController
    {
        private ValueChannel fieldOfView;
        private ValueChannel tiltFactor;
        private ValueChannel translationX;
        private ValueChannel translationY;
        private ValueChannel translationZ;
        private ValueChannel rotationX;
        private ValueChannel rotationY;
        private ValueChannel rotationZ;
        private ValueChannel scaleX;
        private ValueChannel scaleY;
        private ValueChannel scaleZ;
        private ValueChannel near = 0.01f;
        private ValueChannel far = 10000f;

        public ValueChannel Near
        {
            get => near;
            set
            {
                near = value;
                EnableCameraModule = true;
            }
        }

        public ValueChannel Far
        {
            get => far;
            set
            {
                far = value;
                EnableCameraModule = true;
            }
        }

        [EmmyDoc("Channel for the camera's field of view. Value of this channel is added to the camera's internal field of view value (Default is 0)")]
        public ValueChannel FieldOfView
        {
            get => fieldOfView;
            set
            {
                fieldOfView = value;
                EnableCameraModule = true;
            }
        }

        [EmmyDoc("Channel for the camera's tilt factor. Camera tilting rotation is multipled with the value of this channel (Default is 1)")]
        public ValueChannel TiltFactor
        {
            get => tiltFactor;
            set
            {
                tiltFactor = value;
                EnableCameraModule = true;
            }
        }

        public ValueChannel TranslationX
        {
            get => translationX;
            set
            {
                translationX = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel TranslationY
        {
            get => translationY;
            set
            {
                translationY = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel TranslationZ
        {
            get => translationZ;
            set
            {
                translationZ = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel RotationX
        {
            get => rotationX;
            set
            {
                rotationX = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel RotationY
        {
            get => rotationY;
            set
            {
                rotationY = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel RotationZ
        {
            get => rotationZ;
            set
            {
                rotationZ = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel ScaleX
        {
            get => scaleX;
            set
            {
                scaleX = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel ScaleY
        {
            get => scaleY;
            set
            {
                scaleY = value;
                EnablePositionModule = true;
            }
        }

        public ValueChannel ScaleZ
        {
            get => scaleZ;
            set
            {
                scaleZ = value;
                EnablePositionModule = true;
            }
        }

        [MoonSharpHidden] public float DefaultFieldOfView => 0;

        [MoonSharpHidden] public Vector3 DefaultTranslation => Vector3.zero;

        [MoonSharpHidden] public Quaternion DefaultRotation => Quaternion.identity;

        [MoonSharpHidden] public Vector3 DefaultScale => Vector3.one;

        public bool EnableCameraModule { get; set; }

        public bool EnablePositionModule { get; set; }

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