namespace ArcCreate.Gameplay.Data
{
    public abstract class Note : ArcEvent
    {
        public double FloorPosition { get; set; }

        public virtual int TotalCombo => 1;

        public virtual int ComboAt(int timing) => (timing >= Timing) ? 1 : 0;

        public virtual int CompareTo(Note other)
            => TimingGroup.CompareTo(other.Timing);

        protected float ZPos(double floorPosition)
            => ArcFormula.FloorPositionToZ(FloorPosition - floorPosition);
    }
}
