using ArcCreate.Gameplay.Chart;

namespace ArcCreate.Gameplay.Data
{
    /// <summary>
    /// Base class for all chart events.
    /// </summary>
    public abstract class ArcEvent
    {
        private int timingGroup = int.MinValue;

        /// <summary>
        /// Gets or sets the note's timing.
        /// </summary>
        public int Timing { get; set; }

        /// <summary>
        /// Gets or sets the note's timing group.
        /// Upon notifying manager classes of these changes with <see cref="IChartControl.UpdateEvents(System.Collections.Generic.IEnumerable{ArcEvent})"/>,
        /// the note will be moved to the correct timing group.
        /// </summary>
        public int TimingGroup
        {
            get => timingGroup;
            set
            {
                if (TimingGroupChangedFrom == int.MinValue)
                {
                    TimingGroupChangedFrom = timingGroup;
                }

                timingGroup = value;
            }
        }

        public TimingGroup TimingGroupInstance => Services.Chart.GetTimingGroup(TimingGroup);

        public bool NoInput => TimingGroupInstance.GroupProperties.NoInput;

        /// <summary>
        /// Gets a value indicating the previous timing group value of this note.
        /// Used to relocate the note whenever there's a change in its timing group property.
        /// </summary>
        public int TimingGroupChangedFrom { get; internal set; } = int.MinValue;

        /// <summary>
        /// Gets a value indicating whether this note's timing group property has been changed
        /// and it has not been moved to the correct timing group yet.
        /// </summary>
        public bool TimingGroupChanged => TimingGroupChangedFrom != int.MinValue && TimingGroupChangedFrom != TimingGroup;

        public void ResetTimingGroupChangedFrom() => TimingGroupChangedFrom = int.MinValue;

        /// <summary>
        /// Get a new event instance with the same values.
        /// Note that the new event is only a copy of the note's data, and will not be added to the chart.
        /// </summary>
        /// <returns>The new event.</returns>
        public abstract ArcEvent Clone();

        /// <summary>
        /// Assign values to this note.
        /// </summary>
        /// <param name="newValues">The event to copy values from.</param>
        public virtual void Assign(ArcEvent newValues)
        {
            Timing = newValues.Timing;
            TimingGroup = newValues.TimingGroup;
        }
    }
}
