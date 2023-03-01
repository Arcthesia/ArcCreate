using System;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class TextKey
    {
        public int Timing { get; set; }

        public char[] Value { get; set; }

        public string EasingString { get; set; }

        public Func<float, float, float, float> Easing { get; set; }

        public int TransitionFrom { get; set; }

        public int OverrideIndex { get; set; } = 0;

        public void Deserialize(string str)
        {
            string[] split = str.Split(',');
            Timing = UnityEngine.Mathf.RoundToInt(float.Parse(split[0]));
            Value = split[1].ToCharArray();
            TransitionFrom = UnityEngine.Mathf.RoundToInt(float.Parse(split[2]));
            EasingString = split[3];
        }

        public object Serialize()
        {
            return $"{Timing},{Value},{TransitionFrom},{EasingString}";
        }
    }
}