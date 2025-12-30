namespace ArcCreate.Gameplay.Data
{
    public class FragmentEvent : ArcEvent
    {
        public string File { get; set; }

        public override ArcEvent Clone()
        {
            return new FragmentEvent()
            {
                Timing = Timing,
                TimingGroup = TimingGroup,
                File = File,
            };
        }
    }
}