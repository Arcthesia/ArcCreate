using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Utility.Lua
{
    [MoonSharpUserData]
    public struct XYZ
    {
        public float X;
        public float Y;
        public float Z;

        public XYZ(Vector3 unityVector)
        {
            X = unityVector.x;
            Y = unityVector.y;
            Z = unityVector.z;
        }

        public XYZ(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static XYZ operator +(XYZ xyz1, XYZ xyz2)
        {
            return new XYZ(xyz1.X + xyz2.X, xyz1.Y + xyz2.Y, xyz1.Z + xyz2.Z);
        }

        public static XYZ operator -(XYZ xyz1, XYZ xyz2)
        {
            return new XYZ(xyz1.X - xyz2.X, xyz1.Y - xyz2.Y, xyz1.Z - xyz2.Z);
        }

        public static XYZ operator +(XYZ xyz)
        {
            return new XYZ(xyz.X, xyz.Y, xyz.Z);
        }

        public static XYZ operator -(XYZ xyz)
        {
            return xyz * -1;
        }

        public static XYZ operator *(XYZ xyz, float num)
        {
            return new XYZ(xyz.X * num, xyz.Y * num, xyz.Z * num);
        }

        public static XYZ operator *(float num, XYZ xyz)
        {
            return xyz * num;
        }

        public static XYZ operator /(XYZ xyz, float num)
        {
            return new XYZ(xyz.X / num, xyz.Y / num, xyz.Z / num);
        }

        public XYZ MirrorX(float axis = 0f)
        {
            return new XYZ(axis + axis - X, Y, Z);
        }

        public XYZ MirrorY(float axis = 0f)
        {
            return new XYZ(X, axis + axis - Y, Z);
        }

        public XYZ MirrorZ(float axis = 0f)
        {
            return new XYZ(X, Y, axis + axis - Z);
        }

        public override string ToString()
        {
            return $"({X:f2}:{Y:f2}:{Z:f2})";
        }

        [MoonSharpHidden]
        public Vector3 ToVector()
        {
            return new Vector3(X, Y, Z);
        }
    }
}