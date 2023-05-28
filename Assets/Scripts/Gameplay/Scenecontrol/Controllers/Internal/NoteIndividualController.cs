using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class NoteIndividualController : Controller, INoteIndividualController, IColorController
    {
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

        public bool EnableColorModule { get; set; }

        public Color DefaultColor => Color.white;

        [MoonSharpHidden]
        private NoteProperties CurrentProperties
        {
            get
            {
                return TimingGroup.GroupProperties.IndividualOverrides.PropertiesFor(CurrentNote);
            }
        }

        [EmmyDoc("Create a channel which returns the timing of the given note")]
        public static NoteTimingChannel NoteTiming()
            => new NoteTimingChannel();

        [MoonSharpHidden]
        public void Clear()
        {
            TimingGroup.GroupProperties.IndividualOverrides.Clear();
        }

        [MoonSharpHidden]
        public void UpdateColor(Color color)
        {
            if (CurrentNote is null)
            {
                Clear();
                return;
            }

            TimingGroup.GroupProperties.IndividualOverrides.UseColor = true;
            CurrentProperties.Color = color;
        }
    }
}