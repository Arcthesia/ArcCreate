using System;
using System.Text;

namespace ArcCreate.Gameplay.Scenecontrol
{
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
            int i = str.IndexOf(',');
            int k = str.LastIndexOf(',');
            int j = str.LastIndexOf(',', k - 1);
            string timingString = str.Substring(0, i);
            string valueString = str.Substring(i + 1, j - i - 1);
            string fromString = str.Substring(j + 1, k - j - 1);
            string easingString = str.Substring(k + 1);
            Timing = UnityEngine.Mathf.RoundToInt(float.Parse(timingString));
            Value = valueString.ToCharArray();
            TransitionFrom = UnityEngine.Mathf.RoundToInt(float.Parse(fromString));
            EasingString = easingString;
        }

        public object Serialize()
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < Value.Length; i++)
            {
                str.Append(Value[i]);
            }

            return $"{Timing},{str},{TransitionFrom},{EasingString}";
        }
    }
}