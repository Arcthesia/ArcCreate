using System;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public interface INote<Behaviour> : IComparable<INote<Behaviour>>
        where Behaviour : MonoBehaviour
    {
        int Timing { get; }

        double FloorPosition { get; }

        /// <summary>
        /// Gets a value indicating whether or not there's an instance assigned to this note.
        /// </summary>
        /// <value>Whether or not there's an instance assigned to this note.</value>
        bool IsAssignedInstance { get; }

        /// <summary>
        /// Gets the total combo count of this note. Should be 1 for tap notes.
        /// </summary>
        /// <value>The total combo count.</value>
        int TotalCombo { get; }

        /// <summary>
        /// Assign a MonoBehaviour instance from the pool to this note.
        /// </summary>
        /// <param name="instance">The instance.</param>
        void AssignInstance(Behaviour instance);

        /// <summary>
        /// Remove the instance from this note to return it to the pool.
        /// </summary>
        /// <returns>The instance that was just removed.</returns>
        Behaviour RevokeInstance();

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
        void UpdateInstance(int currentTiming, double currentFloorPosition, GroupProperties groupProperties);
    }
}
