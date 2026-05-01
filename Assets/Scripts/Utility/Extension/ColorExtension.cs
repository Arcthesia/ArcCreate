using UnityEngine;

namespace ArcCreate.Utility.Extension
{
    public static class ColorExtension
    {
        public static bool ConvertHexToColor(this string str, out Color color)
        {
            if (ColorUtility.TryParseHtmlString(str, out Color c))
            {
                color = c;
                return true;
            }

            color = Color.black;
            return false;
        }

        public static string ConvertToHexCode(this Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGBA(color);
        }
        
        public static int ToArgbInt(this Color color)
        {
            int a = (int)(color.a * 255f + 0.5f);
            int r = (int)(color.r * 255f + 0.5f);
            int g = (int)(color.g * 255f + 0.5f);
            int b = (int)(color.b * 255f + 0.5f);

            return (a << 24) | (r << 16) | (g << 8) | b;
        }

        public static Color ToArgbColor(this int argb)
        {
            float a = ((argb >> 24) & 0xFF) / 255f;
            float r = ((argb >> 16) & 0xFF) / 255f;
            float g = ((argb >> 8) & 0xFF) / 255f;
            float b = (argb & 0xFF) / 255f;

            return new Color(r, g, b, a);
        }

        public static int ToRgbaInt(this Color color)
        {
            int r = (int)(color.r * 255f + 0.5f);
            int g = (int)(color.g * 255f + 0.5f);
            int b = (int)(color.b * 255f + 0.5f);
            int a = (int)(color.a * 255f + 0.5f);

            return (r << 24) | (g << 16) | (b << 8) | a;
        }

        public static Color ToRgbaColor(this int rgba)
        {
            float r = ((rgba >> 24) & 0xFF) / 255f;
            float g = ((rgba >> 16) & 0xFF) / 255f;
            float b = ((rgba >> 8) & 0xFF) / 255f;
            float a = (rgba & 0xFF) / 255f;

            return new Color(r, g, b, a);
        }
    }
}