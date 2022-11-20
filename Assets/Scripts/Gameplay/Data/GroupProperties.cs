using ArcCreate.ChartFormat;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class GroupProperties
    {
        public GroupProperties()
        {
            // SkinOverride = NoteSkinOverride.Default;
            FadingHolds = false;
            NoInput = false;
            NoClip = false;
            AngleX = 0;
            AngleY = 0;
        }

        public GroupProperties(RawTimingGroup raw)
        {
            // SkinOverride = (NoteSkinOverride)(int)raw.Side;
            FadingHolds = raw.FadingHolds;
            NoInput = raw.NoInput;
            NoClip = raw.NoClip;
            AngleX = raw.AngleX;
            AngleY = raw.AngleY;
        }

        // public NoteSkinOverride SkinOverride;
        public Color Color { get; set; } = Color.white;

        public Vector3 ScaleIndividual { get; set; } = Vector3.one;

        public Quaternion RotationIndividual { get; set; } = Quaternion.identity;

        public bool NoInput { get; set; } = false;

        public bool NoClip { get; set; } = false;

        public bool FadingHolds { get; set; } = false;

        public float AngleX { get; set; } = 0;

        public float AngleY { get; set; } = 0;

        public float SCAngleX { get; set; } = 0;

        public float SCAngleY { get; set; } = 0;

        public Vector3 FallDirection
        {
            get
            {
                float angleXf = 90.0f - AngleX - SCAngleX;
                float angleYf = AngleY + SCAngleY;

                float x = Mathf.Sin(angleXf * Mathf.Deg2Rad) * Mathf.Sin(angleYf * Mathf.Deg2Rad);
                float y = Mathf.Cos(angleXf * Mathf.Deg2Rad);
                float z = Mathf.Sin(angleXf * Mathf.Deg2Rad) * Mathf.Cos(angleYf * Mathf.Deg2Rad);
                return new Vector3(x, y, z);
            }
        }

        public RawTimingGroup ToRaw()
        {
            return new RawTimingGroup
            {
                // Side = (SideOverride)(int)SkinOverride,
                FadingHolds = FadingHolds,
                NoInput = NoInput,
                NoClip = NoClip,
                AngleX = AngleX,
                AngleY = AngleY,
            };
        }
    }
}