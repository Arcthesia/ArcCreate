using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Utility.Lua
{
    [MoonSharpUserData]
    public struct XY
    {
        public float X;
        public float Y;

        public XY(float x, float y)
        {
            X = x;
            Y = y;
        }

        public XY(XY xy)
        {
            X = xy.X;
            Y = xy.Y;
        }

        public XY(Vector2 v)
        {
            X = v.x;
            Y = v.y;
        }

        public static XY operator +(XY xy1, XY xy2)
        {
            return new XY(xy1.X + xy2.X, xy1.Y + xy2.Y);
        }

        public static XY operator -(XY xy1, XY xy2)
        {
            return new XY(xy1.X - xy2.X, xy1.Y - xy2.Y);
        }

        public static XY operator +(XY xy)
        {
            return new XY(xy);
        }

        public static XY operator -(XY xy)
        {
            return xy * -1;
        }

        public static XY operator *(XY xy, float num)
        {
            return new XY(xy.X * num, xy.Y * num);
        }

        public static XY operator *(float num, XY xy)
        {
            return xy * num;
        }

        public static XY operator /(XY xy, float num)
        {
            return new XY(xy.X / num, xy.Y / num);
        }

        public XY MirrorX(float axis = 0.5f)
        {
            return new XY(axis + axis - X, Y);
        }

        public XY MirrorY(float axis = 0.5f)
        {
            return new XY(X, axis + axis - Y);
        }

        public override string ToString()
        {
            return $"({X:f2}:{Y:f2})";
        }

        [MoonSharpHidden]
        public Vector2 ToVector()
        {
            return new Vector2(X, Y);
        }
    }
}