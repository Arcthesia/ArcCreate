namespace ArcCreate.Gameplay.Data
{
    public class ArcTap : Note
    {
        public Arc Arc { get; set; }

        public float WorldX => Arc.WorldXAt(Timing);

        public float WorldY => Arc.WorldYAt(Timing);

        public string Sfx => Arc.Sfx;

        public override ArcEvent Clone()
        {
            return new ArcTap()
            {
                Timing = Timing,
                Arc = Arc,
            };
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            ArcTap e = newValues as ArcTap;
            Arc = e.Arc;
        }
    }
}
