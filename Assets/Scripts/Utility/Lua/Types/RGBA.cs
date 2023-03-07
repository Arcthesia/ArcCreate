using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Utilities.Lua
{
    [MoonSharpUserData]
    public struct RGBA
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public RGBA(Color color)
        {
            R = color.r * 255;
            G = color.g * 255;
            B = color.b * 255;
            A = color.a * 255;
        }

        public RGBA(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static RGBA operator +(RGBA c1, RGBA c2)
        {
            return new RGBA(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B, c1.A + c2.A);
        }

        public static RGBA operator -(RGBA c1, RGBA c2)
        {
            return new RGBA(c1.R - c2.R, c1.G - c2.G, c1.B - c2.B, c1.A - c2.A);
        }

        public static RGBA operator +(RGBA c)
        {
            return new RGBA(c.R, c.G, c.B, c.A);
        }

        public static RGBA operator -(RGBA c)
        {
            return c * -1;
        }

        public static RGBA operator *(RGBA c, float num)
        {
            return new RGBA(c.R * num, c.G * num, c.B * num, c.A * num);
        }

        public static RGBA operator *(float num, RGBA c)
        {
            return c * num;
        }

        public static RGBA operator /(RGBA c, float num)
        {
            return new RGBA(c.R / num, c.G / num, c.B / num, c.A / num);
        }

        public override string ToString()
        {
            return $"({R:f2}:{G:f2}:{B:f2}:{A:f2})";
        }

        [MoonSharpHidden]
        public Color ToColor()
        {
            return new Color(R / 255f, G / 255f, B / 255f, A / 255f);
        }
    }
}