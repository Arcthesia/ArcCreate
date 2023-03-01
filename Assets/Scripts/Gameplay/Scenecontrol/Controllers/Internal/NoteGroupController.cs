using System;
using ArcCreate.Gameplay.Chart;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class NoteGroupController : Controller, IPositionController, INoteGroupController, IColorController
    {
        [MoonSharpHidden] public TimingGroup TimingGroup { get; set; }

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

        public ValueChannel RotationIndividualX { get; set; }

        public ValueChannel RotationIndividualY { get; set; }

        public ValueChannel RotationIndividualZ { get; set; }

        public ValueChannel ScaleIndividualX { get; set; }

        public ValueChannel ScaleIndividualY { get; set; }

        public ValueChannel ScaleIndividualZ { get; set; }

        public ValueChannel ColorR { get; set; }

        public ValueChannel ColorG { get; set; }

        public ValueChannel ColorB { get; set; }

        public ValueChannel ColorH { get; set; }

        public ValueChannel ColorS { get; set; }

        public ValueChannel ColorV { get; set; }

        public ValueChannel ColorA { get; set; }

        public Vector3 DefaultTranslation => Vector3.zero;

        public Quaternion DefaultRotation => Quaternion.identity;

        public Vector3 DefaultScale => Vector3.one;

        public Color DefaultColor => Color.white;

        public void UpdateColor(Color color)
        {
            TimingGroup.GroupProperties.Color = color;
        }

        public void UpdateNoteGroup(Quaternion rotation, Vector3 scale, Vector2 angle)
        {
            TimingGroup.GroupProperties.SCAngleX = angle.x;
            TimingGroup.GroupProperties.SCAngleY = angle.y;
            TimingGroup.GroupProperties.RotationIndividual = rotation;
            TimingGroup.GroupProperties.ScaleIndividual = scale;
        }

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