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
            string[] split = str.Split(',');
            Timing = UnityEngine.Mathf.RoundToInt(float.Parse(split[0]));
            Value = split[1].ToCharArray();
            TransitionFrom = UnityEngine.Mathf.RoundToInt(float.Parse(split[2]));
            EasingString = split[3];
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