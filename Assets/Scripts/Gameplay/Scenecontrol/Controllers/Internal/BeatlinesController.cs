using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Controller for beatlines display")]
    public class BeatlinesController : Controller, IPositionController
    {
        public ValueChannel TranslationX { get; set; }

        public ValueChannel TranslationY { get; set; }

        public ValueChannel TranslationZ { get; set; }

        public ValueChannel RotationX { get; set; }

        public ValueChannel RotationY { get; set; }

        public ValueChannel RotationZ { get; set; }

        public ValueChannel ScaleX { get; set; }

        public ValueChannel ScaleY { get; set; }

        public ValueChannel ScaleZ { get; set; }

        public ValueChannel AngleX { get; set; }

        public ValueChannel AngleY { get; set; }

        public Vector3 DefaultTranslation => Vector3.zero;

        public Quaternion DefaultRotation => Quaternion.identity;

        public Vector3 DefaultScale => Vector3.one;

        [MoonSharpHidden]
        public void UpdatePosition(Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            translation.x *= -1;
            translation.z *= -1;
            rotation.x *= -1;
            rotation.z *= -1;
            transform.localPosition = translation;
            transform.localRotation = rotation;
            transform.localScale = scale;
        }
    }
}