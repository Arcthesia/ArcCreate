namespace ArcCreate.Gameplay.Data
{
    public interface ILongNote : INote
    {
        /// <summary>
        /// Gets the note's end timing.
        /// </summary>
        int EndTiming { get; }

        /// <summary>
        /// Gets the floor position value tied to the end timing of this note.
        /// Calculated with <see cref="Chart.TimingGroup.GetFloorPosition"/>.
        /// </summary>
        double EndFloorPosition { get; }
    }
}
