using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Lua;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyGroup("Macros")]
    public class LuaCamera : LuaChartEvent
    {
        public XYZ Move { get; set; }

        public XYZ Rotate { get; set; }

        public float X
        {
            get => Move.X;
            set
            {
                XYZ xyz = Move;
                xyz.X = value;
                Move = xyz;
            }
        }

        public float Y
        {
            get => Move.Y;
            set
            {
                XYZ xyz = Move;
                xyz.Y = value;
                Move = xyz;
            }
        }

        public float Z
        {
            get => Move.Z;
            set
            {
                XYZ xyz = Move;
                xyz.Z = value;
                Move = xyz;
            }
        }

        public float Rx
        {
            get => Rotate.X;
            set
            {
                XYZ xyz = Rotate;
                xyz.X = value;
                Rotate = xyz;
            }
        }

        public float Ry
        {
            get => Rotate.Y;
            set
            {
                XYZ xyz = Rotate;
                xyz.Y = value;
                Rotate = xyz;
            }
        }

        public float Rz
        {
            get => Rotate.Z;
            set
            {
                XYZ xyz = Rotate;
                xyz.Z = value;
                Rotate = xyz;
            }
        }

        [MoonSharpHidden]
        public Gameplay.Data.CameraType CameraType { get; set; }

        public int Duration { get; set; }

        public string Type
        {
            get => CameraType.ToCameraString();
            set => CameraType = value.ToCameraType();
        }

        public override LuaChartEvent Copy()
        {
            return new LuaCamera
            {
                Move = Move,
                Rotate = Rotate,
                CameraType = CameraType,
                Duration = Duration,
                TimingGroup = TimingGroup,
            };
        }

        [MoonSharpHidden]
        public override ArcEvent CreateInstance()
        {
            return new CameraEvent
            {
                Timing = Mathf.RoundToInt(Timing),
                Duration = Duration,
                CameraType = CameraType,
                Move = Move.ToVector(),
                Rotate = Rotate.ToVector(),
                TimingGroup = TimingGroup,
            };
        }
    }
}