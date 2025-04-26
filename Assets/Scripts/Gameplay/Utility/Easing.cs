using System;
using System.Collections.Generic;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Utility
{
    [MoonSharpUserData]
    [EmmySingleton]
    public class Easing
    {
        private const float Pi = Mathf.PI;

        private static readonly Dictionary<string, Func<float, float, float, float>> StringMapping = new Dictionary<string, Func<float, float, float, float>>()
        {
            { "linear", Linear },
            { "l", Linear },
            { "inconstant", InConst },
            { "inconst", InConst },
            { "cnsti", InConst },
            { "outconstant", OutConst },
            { "outconst", OutConst },
            { "cnsto", OutConst },
            { "inoutconstant", InOutConst },
            { "inoutconst", InOutConst },
            { "cnstb", InOutConst },
            { "insine", InSine },
            { "si", InSine },
            { "outsine", OutSine },
            { "so", OutSine },
            { "inoutsine", InOutSine },
            { "b", InOutSine },
            { "inquadratic", InQuad },
            { "inquad", InQuad },
            { "2i", InQuad },
            { "outquadratic", OutQuad },
            { "outquad", OutQuad },
            { "2o", OutQuad },
            { "inoutquadratic", InOutQuad },
            { "inoutquad", InOutQuad },
            { "2b", InOutQuad },
            { "incubic", InCubic },
            { "3i", InCubic },
            { "outcubic", OutCubic },
            { "outcube", OutCubic },
            { "3o", OutCubic },
            { "inoutcubic", InOutCubic },
            { "inoutcube", InOutCubic },
            { "3b", InOutCubic },
            { "inquartic", InQuart },
            { "inquart", InQuart },
            { "4i", InQuart },
            { "outquartic", OutQuart },
            { "outquart", OutQuart },
            { "4o", OutQuart },
            { "inoutquartic", InOutQuart },
            { "inoutquart", InOutQuart },
            { "4b", InOutQuart },
            { "inquintic", InQuint },
            { "inquint", InQuint },
            { "5i", InQuint },
            { "outquintic", OutQuint },
            { "outquint", OutQuint },
            { "5o", OutQuint },
            { "inoutquintic", InOutQuint },
            { "inoutquint", InOutQuint },
            { "5b", InOutQuint },
            { "inexponential", InExpo },
            { "inexpo", InExpo },
            { "exi", InExpo },
            { "outexponential", OutExpo },
            { "outexpo", OutExpo },
            { "exo", OutExpo },
            { "inoutexponential", InOutExpo },
            { "inoutexpo", InOutExpo },
            { "exb", InOutExpo },
            { "incircle", InCirc },
            { "incirc", InCirc },
            { "ci", InCirc },
            { "outcircle", OutCirc },
            { "outcirc", OutCirc },
            { "co", OutCirc },
            { "inoutcircle", InOutCirc },
            { "inoutcirc", InOutCirc },
            { "cb", InOutCirc },
            { "inback", InBack },
            { "bki", InBack },
            { "outback", OutBack },
            { "bko", OutBack },
            { "inoutback", InOutBack },
            { "bkb", InOutBack },
            { "inelastic", InElastic },
            { "eli", InElastic },
            { "outelastic", OutElastic },
            { "elo", OutElastic },
            { "inoutelastic", InOutElastic },
            { "elb", InOutElastic },
            { "inbounce", InBounce },
            { "bni", InBounce },
            { "outbounce", OutBounce },
            { "bno", OutBounce },
            { "inoutbounce", InOutBounce },
            { "bnb", OutBounce },
        };

        public static float Linear(float start, float end, float x)
        {
            return start + ((end - start) * x);
        }

        public static float InConst(float start, float end, float x)
        {
            return start;
        }

        public static float OutConst(float start, float end, float x)
        {
            return end;
        }

        public static float InOutConst(float start, float end, float x)
        {
            return (x >= 0.5) ? end : start;
        }

        public static float InSine(float start, float end, float x)
        {
            return start + ((end - start) * (1 - Cos(x * Pi / 2)));
        }

        public static float OutSine(float start, float end, float x)
        {
            return start + ((end - start) * Sin(x * Pi / 2));
        }

        public static float InOutSine(float start, float end, float x)
        {
            return start + ((end - start) * (1 - Cos(x * Pi)) / 2);
        }

        public static float InQuad(float start, float end, float x)
        {
            return start + ((end - start) * x * x);
        }

        public static float OutQuad(float start, float end, float x)
        {
            return start + ((end - start) * (1 - ((1 - x) * (1 - x))));
        }

        public static float InOutQuad(float start, float end, float x)
        {
            return start + ((end - start) * (x < 0.5 ? (2 * x * x) : (1 - ((2 - (2 * x)) * (2 - (2 * x)) / 2))));
        }

        public static float InCubic(float start, float end, float x)
        {
            return start + ((end - start) * (x * x * x));
        }

        public static float OutCubic(float start, float end, float x)
        {
            return start + ((end - start) * (1 - Pow(1 - x, 3)));
        }

        public static float InOutCubic(float start, float end, float x)
        {
            return start + ((end - start) * (x < 0.5 ? 4 * x * x * x : 1 - (Pow((-2 * x) + 2, 3) / 2)));
        }

        public static float InQuart(float start, float end, float x)
        {
            return start + ((end - start) * (x * x * x * x));
        }

        public static float OutQuart(float start, float end, float x)
        {
            return start + ((end - start) * (1 - Pow(1 - x, 4)));
        }

        public static float InOutQuart(float start, float end, float x)
        {
            return start + ((end - start) * (x < 0.5 ? 8 * x * x * x * x : 1 - (Pow((-2 * x) + 2, 4) / 2)));
        }

        public static float InQuint(float start, float end, float x)
        {
            return start + ((end - start) * (x * x * x * x * x));
        }

        public static float OutQuint(float start, float end, float x)
        {
            return start + ((end - start) * (1 - Pow(1 - x, 5)));
        }

        public static float InOutQuint(float start, float end, float x)
        {
            return start + ((end - start) * (x < 0.5 ? 16 * x * x * x * x * x : 1 - (Pow((-2 * x) + 2, 5) / 2)));
        }

        public static float InExpo(float start, float end, float x)
        {
            return start + ((end - start) * (x == 0 ? 0 : Pow(2, (10 * x) - 10)));
        }

        public static float OutExpo(float start, float end, float x)
        {
            return start + ((end - start) * (x == 1 ? 1 : 1 - Pow(2, -10 * x)));
        }

        public static float InOutExpo(float start, float end, float x)
        {
            return start + ((end - start) * (x == 0 ? 0 : x == 1 ? 1 : x < 0.5 ? Pow(2, (20 * x) - 10) / 2 : (2 - Pow(2, (-20 * x) + 10)) / 2));
        }

        public static float InCirc(float start, float end, float x)
        {
            return start + ((end - start) * (1 - Sqrt(1 - Pow(x, 2))));
        }

        public static float OutCirc(float start, float end, float x)
        {
            return start + ((end - start) * Sqrt(1 - Pow(x - 1, 2)));
        }

        public static float InOutCirc(float start, float end, float x)
        {
            return start + ((end - start) * (x < 0.5 ? (1 - Sqrt(1 - Pow(2 * x, 2))) / 2 : (Sqrt(1 - Pow((-2 * x) + 2, 2)) + 1) / 2));
        }

        public static float InBack(float start, float end, float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;

            return start + ((end - start) * ((c3 * x * x * x) - (c1 * x * x)));
        }

        public static float OutBack(float start, float end, float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;

            return start + ((end - start) * (1 + (c3 * Pow(x - 1, 3)) + (c1 * Pow(x - 1, 2))));
        }

        public static float InOutBack(float start, float end, float x)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;

            return start + ((end - start) * (x < 0.5 ? Pow(2 * x, 2) * (((c2 + 1) * 2 * x) - c2) / 2 : ((Pow((2 * x) - 2, 2) * (((c2 + 1) * ((x * 2) - 2)) + c2)) + 2) / 2));
        }

        public static float InElastic(float start, float end, float x)
        {
            const float c4 = (2f * Pi) / 3f;

            return start + ((end - start) * (x == 0 ? 0 : x == 1 ? 1 : -Pow(2, (10 * x) - 10) * Sin(((x * 10) - 10.75f) * c4)));
        }

        public static float OutElastic(float start, float end, float x)
        {
            const float c4 = (2f * Pi) / 3f;

            return start + ((end - start) * (x == 0 ? 0 : x == 1 ? 1 : (Pow(2, -10 * x) * Sin(((x * 10) - 0.75f) * c4)) + 1));
        }

        public static float InOutElastic(float start, float end, float x)
        {
            const float c5 = (2 * Pi) / 4.5f;

            return start + ((end - start) * (x == 0 ? 0 : x == 1 ? 1 : x < 0.5 ? -(Pow(2, (20 * x) - 10) * Sin(((20 * x) - 11.125f) * c5)) / 2 : (Pow(2, (-20 * x) + 10) * Sin(((20 * x) - 11.125f) * c5) / 2) + 1));
        }

        public static float InBounce(float start, float end, float x)
        {
            return end - OutBounce(start, end, 1 - x);
        }

        public static float OutBounce(float start, float end, float x)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            return start + ((end - start) * (x < 1 / d1 ? 1 * x * x : x < 2 / d1 ? (1 * (x -= 1.5f / d1) * x) + 0.75f : x < 2.5 / d1 ? (1 * (x -= 2.25f / d1) * x) + 0.9375f : (n1 * (x -= 2.625f / d1) * x) + 0.984375f));
        }

        public static float InOutBounce(float start, float end, float x)
        {
            return start + ((end - start) * (x < 0.5 ? (1 - OutBounce(0, 1, 1 - (2 * x))) / 2 : (1 + OutBounce(0, 1, (2 * x) - 1)) / 2));
        }

        [MoonSharpHidden]
        public static Func<float, float, float, float> FromString(string s)
        {
            if (string.IsNullOrEmpty(s) || !StringMapping.ContainsKey(s))
            {
                return Linear;
            }

            return StringMapping[s];
        }

        private static float Cos(float x)
        {
            return Mathf.Cos(x);
        }

        private static float Sin(float x)
        {
            return Mathf.Sin(x);
        }

        private static float Pow(float x, float y)
        {
            return Mathf.Pow(x, y);
        }

        private static float Sqrt(float x)
        {
            return Mathf.Sqrt(x);
        }
    }
}