using System;
using System.Runtime.CompilerServices;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Skin;
using ArcCreate.Utility;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class GroupProperties
    {
        public GroupProperties()
        {
            Name = null;
            FileName = null;
            SkinOverride = NoteSkinOverride.Default;
            FadingHolds = false;
            NoInput = false;
            NoClip = false;
            NoHeightIndicator = false;
            NoHead = false;
            NoShadow = false;
            NoArcCap = false;
            AngleX = 0;
            AngleY = 0;
            ArcResolution = 1;
            Editable = true;
        }

        public GroupProperties(RawTimingGroup raw)
        {
            Name = raw.Name;
            FileName = raw.File;
            SkinOverride = (NoteSkinOverride)(int)raw.Side;
            FadingHolds = raw.FadingHolds;
            NoInput = raw.NoInput;
            NoClip = raw.NoClip;
            NoHeightIndicator = raw.NoHeightIndicator;
            NoHead = raw.NoHead;
            NoShadow = raw.NoShadow;
            NoArcCap = raw.NoArcCap;
            AngleX = raw.AngleX;
            AngleY = raw.AngleY;
            ArcResolution = raw.ArcResolution;
            Editable = raw.Editable;
        }

        public string Name { get; set; }

        public string FileName { get; set; }

        public bool Editable { get; set; }

        public NoteSkinOverride SkinOverride { get; set; }

        public Color Color { get; set; } = Color.white;

        public Vector3 ScaleIndividual { get; set; } = Vector3.one;

        public Quaternion RotationIndividual { get; set; } = Quaternion.identity;

        public bool NoInput { get; set; } = false;

        public bool NoClip { get; set; } = false;

        public bool NoHeightIndicator { get; set; } = false;

        public bool NoHead { get; set; } = false;

        public bool NoShadow { get; set; } = false;

        public bool NoArcCap { get; set; } = false;

        public bool FadingHolds { get; set; } = false;

        public float AngleX { get; set; } = 0;

        public float AngleY { get; set; } = 0;

        public float ArcResolution { get; set; } = 0;

        public float SCAngleX { get; set; } = 0;

        public float SCAngleY { get; set; } = 0;

        public TRS GroupTransform { get; set; } = TRS.identity;

        public bool Visible { get; set; } = true;

        public NoteIndividualProperties IndividualOverrides { get; set; } = new NoteIndividualProperties();

        public Vector3 GetFallDirection(Note note = null)
        {
            float angleXf = 90.0f - GetAngleX(note);
            float angleYf = GetAngleY(note);

            float x = Mathf.Sin(angleXf * Mathf.Deg2Rad) * Mathf.Sin(angleYf * Mathf.Deg2Rad);
            float y = -Mathf.Cos(angleXf * Mathf.Deg2Rad);
            float z = Mathf.Sin(angleXf * Mathf.Deg2Rad) * Mathf.Cos(angleYf * Mathf.Deg2Rad);

            return new Vector3(x, y, z);
        }

        public Color GetColor(Note note)
        {
            return Color * GetNIProperty(note, Color.white, ni => ni.UseColor, n => n.Color);
        }

        public TRS GetNoteTransform(Note note)
        {
            return GetNIProperty(note, TRS.identity, ni => ni.UsePosition, n => n.Transform)
                + new TRS(default, RotationIndividual, ScaleIndividual);
        }

        public float GetAngleX(Note note)
        {
            return AngleX + SCAngleX + GetNIProperty(note, 0, ni => ni.UseAngle, n => n.Angles.x);
        }

        public float GetAngleY(Note note)
        {
            return AngleY + SCAngleY + GetNIProperty(note, 0, ni => ni.UseAngle, n => n.Angles.y);
        }

        public RawTimingGroup ToRaw()
        {
            return new RawTimingGroup
            {
                Name = Name,
                File = FileName,
                Side = (SideOverride)(int)SkinOverride,
                FadingHolds = FadingHolds,
                NoInput = NoInput,
                NoHeightIndicator = NoHeightIndicator,
                NoHead = NoHead,
                NoShadow = NoShadow,
                NoClip = NoClip,
                NoArcCap = NoArcCap,
                AngleX = AngleX,
                AngleY = AngleY,
                ArcResolution = ArcResolution,
            };
        }

        /// <summary>
        /// Get a property which may be overriden on a per-note basis.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetNIProperty<T>(Note note, T defaultValue, Predicate<NoteIndividualProperties> nienabled, Func<NoteProperties, T> nivalue)
        {
            if (note is null)
            {
                return defaultValue;
            }

            if (!IndividualOverrides.IsEnabled)
            {
                return defaultValue;
            }

            if (!nienabled(IndividualOverrides))
            {
                return defaultValue;
            }

            var ni = IndividualOverrides.PropertiesFor(note);
            return nivalue(ni);
        }
    }
}