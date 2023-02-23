using System;

namespace ArcCreate.Gameplay.Data
{
    public interface INote : IComparable<INote>
    {
        /// <summary>
        /// Gets the note's timing.
        /// </summary>
        int Timing { get; }

        /// <summary>
        /// Gets the floor position value tied to the timing of this note.
        /// Calculated with <see cref="Chart.TimingGroup.GetFloorPosition"/>.
        /// </summary>
        double FloorPosition { get; }

        /// <summary>
        /// Gets the total combo count of this note. Should be 1 for tap notes.
        /// </summary>
        int TotalCombo { get; }

        /// <summary>
        /// Reset the note's judgement state. Typically called after there's a change in the current timing.
        /// </summary>
        /// <param name="timing">The new timing to reset to.</param>
        void ResetJudgeTo(int timing);

        /// <summary>
        /// Rebuild the note. Called after there's a change in the note's properties.
        /// </summary>
        void Rebuild();

        /// <summary>
        /// Recalculate the note's floor position values.
        /// </summary>
        void RecalculateFloorPosition();

        /// <summary>
        /// Reload the note's skin.
        /// </summary>
        void ReloadSkin();

        /// <summary>
        /// The combo value at timing, assuming the note is played perfectly.
        /// </summary>
        /// <param name="timing">The timing value.</param>
        /// <returns>The combo value.</returns>
        int ComboAt(int timing);

        /// <summary>
        /// Update the note's judgement state. May be called even without an instance attached.
        /// </summary>
        /// <param name="currentTiming">The timing to update to.</param>
        /// <param name="groupProperties">The properties object of the notes' timing group.</param>
        void UpdateJudgement(int currentTiming, GroupProperties groupProperties);

        /// <summary>
        /// Update the note's instance.
        /// It's guaranteed that the note's instance must be attached in order for this to be called.
        /// </summary>
        /// <param name="currentTiming">The timing to update to.</param>
        /// <param name="currentFloorPosition">The floor position corresponding to the timing value.</param>
        /// <param name="groupProperties">The properties object of the notes' timing group.</param>
        void UpdateRender(int currentTiming, double currentFloorPosition, GroupProperties groupProperties);
    }
}
