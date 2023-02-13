namespace ArcCreate.Gameplay.Data
{
    public abstract class Note : ArcEvent
    {
        public abstract bool IsSelected { get; set; }

        public double FloorPosition { get; set; }

        public virtual int TotalCombo { get; protected set; } = 1;

        public virtual int ComboAt(int timing) => (timing >= Timing) ? 1 : 0;

        protected float ZPos(double floorPosition)
            => ArcFormula.FloorPositionToZ(FloorPosition - floorPosition);

        protected virtual void RecalculateFloorPosition()
        {
            FloorPosition = TimingGroupInstance.GetFloorPosition(Timing);
        }
    }
}
