namespace ArcCreate.Gameplay.Data
{
    public abstract class LongNote : ArcEvent
    {
        public int EndTiming { get; set; }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            LongNote e = newValues as LongNote;
            EndTiming = e.EndTiming;
        }
    }
}
