namespace ArcCreate.Gameplay.Data
{
    public abstract class Note : ArcEvent
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not the note is selected.
        /// </summary>
        public abstract bool IsSelected { get; set; }

        public double FloorPosition { get; set; }

        public virtual int TotalCombo { get; protected set; } = 1;

        public virtual int ComboAt(int timing) => (timing >= Timing) ? 1 : 0;

        public virtual void RecalculateFloorPosition()
        {
            FloorPosition = TimingGroupInstance.GetFloorPosition(Timing);
        }

        public float ZPos(double floorPosition)
            => ArcFormula.FloorPositionToZ(FloorPosition - floorPosition);
    }
}
