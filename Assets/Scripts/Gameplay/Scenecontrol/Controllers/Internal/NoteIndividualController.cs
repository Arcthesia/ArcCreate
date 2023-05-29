using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class NoteIndividualController : Controller, INoteIndividualController, IColorController, IPositionController, IAngleController
    {
        // Position
        private ValueChannel translationX;
        private ValueChannel translationY;
        private ValueChannel translationZ;
        private ValueChannel rotationX;
        private ValueChannel rotationY;
        private ValueChannel rotationZ;
        private ValueChannel scaleX;
        private ValueChannel scaleY;
        private ValueChannel scaleZ;

        // Angle
        private ValueChannel angleX;
        private ValueChannel angleY;

        // Color
        private ValueChannel colorR;
        private ValueChannel colorG;
        private ValueChannel colorB;
        private ValueChannel colorH;
        private ValueChannel colorV;
        private ValueChannel colorA;
        private ValueChannel colorS;

        public static Note CurrentNote { get; set; }

        public int GroupNumber => TimingGroup.GroupNumber;

        [MoonSharpHidden] public TimingGroup TimingGroup { get; set; }

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
                EnableAngleModule = true;
            }
        }

        public ValueChannel AngleY
        {
            get => angleY;
            set
            {
                angleY = value;
                EnableAngleModule = true;
            }
        }

        public bool EnableColorModule { get; set; }

        public bool EnablePositionModule { get; set; }

        public bool EnableAngleModule { get; set; }

        public Color DefaultColor => Color.white;

        public Vector3 DefaultTranslation => Vector3.zero;

        public Quaternion DefaultRotation => Quaternion.identity;

        public Vector3 DefaultScale => Vector3.one;

        [MoonSharpHidden]
        private NoteProperties CurrentProperties
        {
            get
            {
                return TimingGroup.GroupProperties.IndividualOverrides.PropertiesFor(CurrentNote);
            }
        }

        [MoonSharpHidden]
        public void UpdateColor(Color color)
        {
            if (CurrentNote is null)
            {
                TimingGroup.GroupProperties.IndividualOverrides.SetAllColors(color);
                return;
            }

            TimingGroup.GroupProperties.IndividualOverrides.UseColor = true;
            CurrentProperties.Color = color;
        }

        [MoonSharpHidden]
        public void UpdatePosition(Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            Matrix4x4 mat = Matrix4x4.TRS(translation, rotation, scale);

            if (CurrentNote is null)
            {
                TimingGroup.GroupProperties.IndividualOverrides.SetAllMatrices(mat);
                return;
            }

            TimingGroup.GroupProperties.IndividualOverrides.UsePosition = true;
            CurrentProperties.Matrix = mat;
        }

        [MoonSharpHidden]
        public void UpdateAngle(float x, float y)
        {
            Vector2 angles = new Vector2(x, y);

            if (CurrentNote is null)
            {
                TimingGroup.GroupProperties.IndividualOverrides.SetAllAngles(angles);
                return;
            }

            TimingGroup.GroupProperties.IndividualOverrides.UseAngle = true;
            CurrentProperties.Angles = angles;
        }
    }
}