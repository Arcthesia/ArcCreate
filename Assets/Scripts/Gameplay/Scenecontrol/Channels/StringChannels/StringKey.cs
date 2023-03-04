namespace ArcCreate.Gameplay.Scenecontrol
{
    public class StringKey
    {
        public int Timing { get; set; }

        public string Value { get; set; }

        public int OverrideIndex { get; set; } = 0;

        public void Deserialize(string str)
        {
            string[] split = str.Split(',');
            Timing = UnityEngine.Mathf.RoundToInt(float.Parse(split[0]));
            Value = split[1];
        }

        public object Serialize()
        {
            return $"{Timing},{Value}";
        }
    }
}