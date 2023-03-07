using System;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public class Key
    {
        public int Timing { get; set; }

        public float Value { get; set; }

        public Func<float, float, float, float> Easing { get; set; }

        public string EasingString { get; set; }

        public int OverrideIndex { get; set; } = 0;

        public string Serialize()
        {
            return $"{Timing},{Value},{EasingString}";
        }

        public void Deserialize(string str)
        {
            string[] split = str.Split(',');
            Timing = UnityEngine.Mathf.RoundToInt(float.Parse(split[0]));
            Value = float.Parse(split[1]);
            EasingString = split[2];
        }
    }
}