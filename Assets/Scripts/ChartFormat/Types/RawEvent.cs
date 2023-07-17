namespace ArcCreate.ChartFormat
{
    public class RawEvent
    {
        public int Timing { get; set; }

        public RawEventType Type { get; set; }

        public int TimingGroup { get; set; }

        public int Line { get; set; }

        public int CharacterStart { get; set; } = 0;

        public int Length { get; set; } = -1;
    }
}