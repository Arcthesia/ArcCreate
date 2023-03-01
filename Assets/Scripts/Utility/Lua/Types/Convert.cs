using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Utilities.Lua
{
    [MoonSharpUserData]
    public class Convert
    {
        public static string RGBAToHex(float r, float g, float b, float a)
        {
            return "#" + ((int)(r * 255)).ToString("X2") + ((int)(g * 255)).ToString("X2") + ((int)(b * 255)).ToString("X2") + ((int)(a * 255)).ToString("X2");
        }

        public static string RGBAToHex(RGBA rgba)
        {
            return RGBAToHex(rgba.R, rgba.G, rgba.B, rgba.A);
        }

        public static string HSVAToHex(HSVA hsva)
        {
            return RGBAToHex(HSVAToRGBA(hsva));
        }

        public static RGBA HexToRGBA(string hex)
        {
            bool converted = ColorUtility.TryParseHtmlString(hex, out Color color);
            if (!converted)
            {
                return new RGBA();
            }

            return new RGBA(color);
        }

        public static HSVA HexToHSVA(string hex)
        {
            return RGBAToHSVA(HexToRGBA(hex));
        }

        public static HSVA RGBAToHSVA(float r, float g, float b, float a)
        {
            Color c = new RGBA(r, g, b, a).ToColor();
            Color.RGBToHSV(c, out float h, out float s, out float v);
            HSVA hsva = new HSVA(h * 360, s, v, a / 255);
            return hsva;
        }

        public static HSVA RGBAToHSVA(RGBA rgba)
        {
            return RGBAToHSVA(rgba.R, rgba.G, rgba.B, rgba.A);
        }

        public static RGBA HSVAToRGBA(float h, float s, float v, float a)
        {
            Color rgb = Color.HSVToRGB(h / 360, s, v);
            RGBA rgba = new RGBA(rgb) { A = a * 255 };
            return rgba;
        }

        public static RGBA HSVAToRGBA(HSVA hsva)
        {
            return HSVAToRGBA(hsva.H, hsva.S, hsva.V, hsva.A);
        }
    }
}