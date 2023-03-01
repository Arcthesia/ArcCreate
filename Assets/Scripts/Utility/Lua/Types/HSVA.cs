using MoonSharp.Interpreter;

namespace ArcCreate.Utilities.Lua
{
    [MoonSharpUserData]
    public class HSVA
    {
        public HSVA()
        {
            H = 0;
            S = 0;
            V = 0;
            A = 1;
        }

        public HSVA(float h, float s, float v, float a)
        {
            H = h;
            S = s;
            V = v;
            A = a;
        }

        public float H { get; set; }

        public float S { get; set; }

        public float V { get; set; }

        public float A { get; set; }

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