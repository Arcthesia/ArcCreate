using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Utilities.Lua
{
    [MoonSharpUserData]
    [EmmySingleton]
    [EmmyDoc("Utility class for converting between types")]
    public class Convert
    {
        [EmmyAlias("RGBAToHex")]
        public static string RGBAToHex(float r, float g, float b, float a)
        {
            return "#" + ((int)(r * 255)).ToString("X2") + ((int)(g * 255)).ToString("X2") + ((int)(b * 255)).ToString("X2") + ((int)(a * 255)).ToString("X2");
        }

        [EmmyAlias("RGBAToHex")]
        public static string RGBAToHex(RGBA rgba)
        {
            return RGBAToHex(rgba.R, rgba.G, rgba.B, rgba.A);
        }

        [EmmyAlias("HSVAToHex")]
        public static string HSVAToHex(HSVA hsva)
        {
            return RGBAToHex(HSVAToRGBA(hsva));
        }

        [EmmyAlias("HexToRGBA")]
        public static RGBA HexToRGBA(string hex)
        {
            bool converted = ColorUtility.TryParseHtmlString(hex, out Color color);
            if (!converted)
            {
                return default;
            }

            return new RGBA(color);
        }

        [EmmyAlias("HexToHSVA")]
        public static HSVA HexToHSVA(string hex)
        {
            return RGBAToHSVA(HexToRGBA(hex));
        }

        [EmmyAlias("RGBAToHSVA")]
        public static HSVA RGBAToHSVA(float r, float g, float b, float a)
        {
            Color c = new RGBA(r, g, b, a).ToColor();
            Color.RGBToHSV(c, out float h, out float s, out float v);
            HSVA hsva = new HSVA(h * 360, s, v, a / 255);
            return hsva;
        }

        [EmmyAlias("RGBAToHSVA")]
        public static HSVA RGBAToHSVA(RGBA rgba)
        {
            return RGBAToHSVA(rgba.R, rgba.G, rgba.B, rgba.A);
        }

        [EmmyAlias("HSVAToRGBA")]
        public static RGBA HSVAToRGBA(float h, float s, float v, float a)
        {
            Color rgb = Color.HSVToRGB(h / 360, s, v);
            RGBA rgba = new RGBA(rgb) { A = a * 255 };
            return rgba;
        }

        [EmmyAlias("HSVAToRGBA")]
        public static RGBA HSVAToRGBA(HSVA hsva)
        {
            return HSVAToRGBA(hsva.H, hsva.S, hsva.V, hsva.A);
        }
    }
}