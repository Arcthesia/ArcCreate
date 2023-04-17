namespace ArcCreate.Gameplay.Scenecontrol
{
    public class StringKey
    {
        public int Timing { get; set; }

        public string Value { get; set; }

        public int OverrideIndex { get; set; } = 0;

        public void Deserialize(string str)
        {
            int i = str.IndexOf(',');
            string timingString = str.Substring(0, i);
            string valueString = str.Substring(i + 1);
            Timing = UnityEngine.Mathf.RoundToInt(float.Parse(timingString));
            Value = valueString;
        }

        public object Serialize()
        {
            return $"{Timing},{Value}";
        }
    }
}