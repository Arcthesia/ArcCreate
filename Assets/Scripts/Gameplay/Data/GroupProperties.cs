using System.Collections.Generic;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Judgement;
using ArcCreate.Gameplay.Skin;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class GroupProperties
    {
        public GroupProperties()
        {
        }

        public GroupProperties(RawTimingGroup raw)
        {
            Name = raw.Name;
            FileName = raw.File;
            SkinOverride = (NoteSkinOverride)(int)raw.Side;
            FadingHolds = raw.FadingHolds;
            IgnoreMirror = raw.IgnoreMirror;
            NoInput = raw.NoInput;
            NoClip = raw.NoClip;
            NoHeightIndicator = raw.NoHeightIndicator;
            NoHead = raw.NoHead;
            NoShadow = raw.NoShadow;
            NoArcCap = raw.NoArcCap;
            NoConnection = raw.NoConnection;
            AngleX = raw.AngleX;
            AngleY = raw.AngleY;
            JudgementOffsetX = raw.JudgementOffsetX;
            JudgementOffsetY = raw.JudgementOffsetY;
            JudgementOffsetZ = raw.JudgementOffsetZ;
            JudgementSizeX = raw.JudgementSizeX;
            JudgementSizeY = raw.JudgementSizeY;
            ArcResolution = raw.ArcResolution;
            DropRate = raw.DropRate;
            Editable = raw.Editable;
            Autoplay = raw.Autoplay;
            foreach (var pair in raw.JudgementMaps)
            {
                JudgementMaps.Add((JudgementResult)(int)pair.Key, (JudgementResult)(int)pair.Value);
            }
        }

        public string Name { get; set; } = null;

        public string FileName { get; set; } = null;

        public bool Editable { get; set; } = true;

        public NoteSkinOverride SkinOverride { get; set; } = NoteSkinOverride.Default;

        public Color Color { get; set; } = Color.white;

        public Vector3 ScaleIndividual { get; set; } = Vector3.one;

        public Quaternion RotationIndividual { get; set; } = Quaternion.identity;

        public bool NoInput { get; set; } = false;

        public bool NoClip { get; set; } = false;

        public bool NoHeightIndicator { get; set; } = false;

        public bool NoHead { get; set; } = false;

        public bool NoShadow { get; set; } = false;

        public bool NoArcCap { get; set; } = false;

        public bool NoConnection { get; set; } = false;

        public bool FadingHolds { get; set; } = false;

        public bool IgnoreMirror { get; set; } = false;

        public bool Autoplay { get; set; } = false;

        public float AngleX { get; set; } = 0;

        public float AngleY { get; set; } = 0;

        public float JudgementSizeX { get; set; } = 1;

        public float JudgementSizeY { get; set; } = 1;

        public float JudgementOffsetX { get; set; } = 0;

        public float JudgementOffsetY { get; set; } = 0;

        public float JudgementOffsetZ { get; set; } = 0;

        public float ArcResolution { get; set; } = 1;

        public float DropRate { get; set; } = 0;

        public float SCAngleX { get; set; } = 0;

        public float SCAngleY { get; set; } = 0;

        public float SCJudgementSizeX { get; set; } = 1;

        public float SCJudgementSizeY { get; set; } = 1;

        public float SCJudgementOffsetX { get; set; } = 0;

        public float SCJudgementOffsetY { get; set; } = 0;

        public float SCJudgementOffsetZ { get; set; } = 0;

        public Matrix4x4 GroupMatrix { get; set; } = Matrix4x4.identity;

        public bool Visible { get; set; } = true;

        public Dictionary<JudgementResult, JudgementResult> JudgementMaps { get; private set; }
            = new Dictionary<JudgementResult, JudgementResult>();

        public Vector3 FallDirection
        {
            get
            {
                float angleXf = 90.0f - AngleX - SCAngleX;
                float angleYf = (AngleY + SCAngleY) * (Settings.MirrorNotes.Value ? -1 : 1);

                float x = Mathf.Sin(angleXf * Mathf.Deg2Rad) * Mathf.Sin(angleYf * Mathf.Deg2Rad);
                float y = -Mathf.Cos(angleXf * Mathf.Deg2Rad);
                float z = Mathf.Sin(angleXf * Mathf.Deg2Rad) * Mathf.Cos(angleYf * Mathf.Deg2Rad);
                return new Vector3(x, y, z);
            }
        }

        public Vector2 CurrentJudgementSize => new Vector2(JudgementSizeX * SCJudgementSizeX, JudgementSizeY * SCJudgementSizeY);

        public Vector3 CurrentJudgementOffset => new Vector3(
            JudgementOffsetX + SCJudgementOffsetX,
            JudgementOffsetY + SCJudgementOffsetY,
            JudgementOffsetZ + SCJudgementOffsetZ);

        public RawTimingGroup ToRaw()
        {
            var rtg = new RawTimingGroup
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
                NoConnection = NoConnection,
                AngleX = AngleX,
                AngleY = AngleY,
                JudgementOffsetX = JudgementOffsetX,
                JudgementOffsetY = JudgementOffsetY,
                JudgementOffsetZ = JudgementOffsetZ,
                JudgementSizeX = JudgementSizeX,
                JudgementSizeY = JudgementSizeY,
                ArcResolution = ArcResolution,
                DropRate = DropRate,
                Autoplay = Autoplay,
                IgnoreMirror = IgnoreMirror,
            };

            foreach (var pair in JudgementMaps)
            {
                rtg.JudgementMaps.Add((JudgementMap)(int)pair.Key, (JudgementMap)(int)pair.Value);
            }

            return rtg;
        }

        public JudgementResult MapJudgementResult(JudgementResult from)
        {
            InputMode inputMode = (InputMode)Settings.InputMode.Value;
            bool isAuto = inputMode == InputMode.Auto || inputMode == InputMode.AutoController;
            if (isAuto || Autoplay)
            {
                return JudgementResult.Max;
            }

            if (JudgementMaps.TryGetValue(from, out JudgementResult to))
            {
                return to;
            }

            return from;
        }
    }
}