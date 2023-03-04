using ArcCreate.Gameplay.Chart;
using ArcCreate.Utilities.Lua;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Controller for a timing group")]
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

        [MoonSharpHidden] public Vector3 DefaultTranslation => Vector3.zero;

        [MoonSharpHidden] public Quaternion DefaultRotation => Quaternion.identity;

        [MoonSharpHidden] public Vector3 DefaultScale => Vector3.one;

        [MoonSharpHidden] public Color DefaultColor => Color.white;

        [MoonSharpHidden]
        public void UpdateColor(Color color)
        {
            TimingGroup.GroupProperties.Color = color;
        }

        [MoonSharpHidden]
        public void UpdateNoteGroup(Quaternion rotation, Vector3 scale, Vector2 angle)
        {
            TimingGroup.GroupProperties.SCAngleX = angle.x;
            TimingGroup.GroupProperties.SCAngleY = angle.y;
            TimingGroup.GroupProperties.RotationIndividual = rotation;
            TimingGroup.GroupProperties.ScaleIndividual = scale;
        }

        [MoonSharpHidden]
        public void UpdatePosition(Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            transform.localPosition = translation;
            transform.localRotation = rotation;
            transform.localScale = scale;
            TimingGroup.GroupProperties.GroupMatrix = Matrix4x4.TRS(translation, rotation, scale);
        }

        protected override void SetActive(bool active)
        {
            base.SetActive(active);
            TimingGroup.GroupProperties.Visible = active;
        }
    }
}