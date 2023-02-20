namespace ArcCreate.Gameplay.Data
{
    public class IncludeEvent : ArcEvent
    {
        public string File { get; set; }

        public override ArcEvent Clone()
        {
            return new IncludeEvent()
            {
                Timing = Timing,
                TimingGroup = TimingGroup,
                File = File,
            };
        }
    }
}