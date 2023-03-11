using ArcCreate.Gameplay.Chart;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Controller for a timing group")]
    public class NoteGroupController : Controller, IPositionController, INoteGroupController, IColorController
    {
        private ValueChannel translationX;
        private ValueChannel translationY;
        private ValueChannel translationZ;
        private ValueChannel rotationX;
        private ValueChannel rotationY;
        private ValueChannel rotationZ;
        private ValueChannel scaleX;
        private ValueChannel scaleY;
        private ValueChannel scaleZ;
        private ValueChannel colorR;
        private ValueChannel colorG;
        private ValueChannel colorB;
        private ValueChannel colorH;
        private ValueChannel colorV;
        private ValueChannel colorA;
        private ValueChannel colorS;
        private ValueChannel angleX;
        private ValueChannel angleY;
        private ValueChannel rotationIndividualX;
        private ValueChannel rotationIndividualY;
        private ValueChannel rotationIndividualZ;
        private ValueChannel scaleIndividualX;
        private ValueChannel scaleIndividualY;
        private ValueChannel scaleIndividualZ;

        [MoonSharpHidden] public TimingGroup TimingGroup { get; set; }

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

        public ValueChannel AngleX
        {
            get => angleX;
            set
            {
                angleX = value;
                EnableNoteGroupModule = true;
            }
        }

        public ValueChannel AngleY
        {
            get => angleY;
            set
            {
                angleY = value;
                EnableNoteGroupModule = true;
            }
        }

        public ValueChannel RotationIndividualX
        {
            get => rotationIndividualX;
            set
            {
                rotationIndividualX = value;
                EnableNoteGroupModule = true;
            }
        }

        public ValueChannel RotationIndividualY
        {
            get => rotationIndividualY;
            set
            {
                rotationIndividualY = value;
                EnableNoteGroupModule = true;
            }
        }

        public ValueChannel RotationIndividualZ
        {
            get => rotationIndividualZ;
            set
            {
                rotationIndividualZ = value;
                EnableNoteGroupModule = true;
            }
        }

        public ValueChannel ScaleIndividualX
        {
            get => scaleIndividualX;
            set
            {
                scaleIndividualX = value;
                EnableNoteGroupModule = true;
            }
        }

        public ValueChannel ScaleIndividualY
        {
            get => scaleIndividualY;
            set
            {
                scaleIndividualY = value;
                EnableNoteGroupModule = true;
            }
        }

        public ValueChannel ScaleIndividualZ
        {
            get => scaleIndividualZ;
            set
            {
                scaleIndividualZ = value;
                EnableNoteGroupModule = true;
            }
        }

        public ValueChannel ColorR
        {
            get => colorR;
            set
            {
                colorR = value;
                EnableColorModule = true;
            }
        }

        public ValueChannel ColorG
        {
            get => colorG;
            set
            {
                colorG = value;
                EnableColorModule = true;
            }
        }

        public ValueChannel ColorB
        {
            get => colorB;
            set
            {
                colorB = value;
                EnableColorModule = true;
            }
        }

        public ValueChannel ColorH
        {
            get => colorH;
            set
            {
                colorH = value;
                EnableColorModule = true;
            }
        }

        public ValueChannel ColorS
        {
            get => colorS;
            set
            {
                colorS = value;
                EnableColorModule = true;
            }
        }

        public ValueChannel ColorV
        {
            get => colorV;
            set
            {
                colorV = value;
                EnableColorModule = true;
            }
        }

        public ValueChannel ColorA
        {
            get => colorA;
            set
            {
                colorA = value;
                EnableColorModule = true;
            }
        }

        [MoonSharpHidden] public Vector3 DefaultTranslation => Vector3.zero;

        [MoonSharpHidden] public Quaternion DefaultRotation => Quaternion.identity;

        [MoonSharpHidden] public Vector3 DefaultScale => Vector3.one;

        [MoonSharpHidden] public Color DefaultColor => Color.white;

        public bool EnablePositionModule { get; set; }

        public bool EnableNoteGroupModule { get; set; }

        public bool EnableColorModule { get; set; }

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
            TimingGroup.GroupProperties.Visible = gameObject.activeInHierarchy;
        }
    }
}