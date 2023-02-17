using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public interface ILongNote<Behaviour> : INote<Behaviour>
        where Behaviour : MonoBehaviour
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
