using MoonSharp.Interpreter;

namespace ArcCreate.Utility.Lua
{
    [MoonSharpUserData]
    public struct HSVA
    {
        public float H;
        public float S;
        public float V;
        public float A;

        public HSVA(float h, float s, float v, float a)
        {
            H = h;
            S = s;
            V = v;
            A = a;
        }

        public static HSVA operator +(HSVA c1, HSVA c2)
        {
            return new HSVA(c1.H + c2.H, c1.S + c2.S, c1.V + c2.V, c1.A + c2.A);
        }

        public static HSVA operator -(HSVA c1, HSVA c2)
        {
            return new HSVA(c1.H - c2.H, c1.S - c2.S, c1.V - c2.V, c1.A - c2.A);
        }

        public static HSVA operator +(HSVA c)
        {
            return new HSVA(c.H, c.S, c.V, c.A);
        }

        public static HSVA operator -(HSVA c)
        {
            return c * -1;
        }

        public static HSVA operator *(HSVA c, float num)
        {
            return new HSVA(c.H * num, c.S * num, c.V * num, c.A * num);
        }

        public static HSVA operator *(float num, HSVA c)
        {
            return c * num;
        }

        public static HSVA operator /(HSVA c, float num)
        {
            return new HSVA(c.H / num, c.S / num, c.V / num, c.A / num);
        }

        public override string ToString()
        {
            return $"({H:f2}:{S:f2}:{V:f2}:{A:f2})";
        }
    }
}