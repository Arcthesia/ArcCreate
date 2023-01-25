using System.Collections.Generic;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay
{
    public interface IChartControl
    {
        List<TimingGroup> TimingGroups { get; }

        /// <summary>
        /// Add events to the currently playing chart.
        /// </summary>
        /// <param name="events">The events to add.</param>
        void AddEvents(IEnumerable<ArcEvent> events);

        /// <summary>
        /// Remove events from the currently playing chart.
        /// </summary>
        /// <param name="events">The events to remove.</param>
        void RemoveEvents(IEnumerable<ArcEvent> events);

        /// <summary>
        /// Notify the system that these events have had their properties changed.
        /// This will force the system to recalculate the notes' floor position, relationship with other notes, etc.
        /// </summary>
        /// <param name="events">The events that has been changed.</param>
        void UpdateEvents(IEnumerable<ArcEvent> events);

        TimingGroup GetTimingGroup(int group);

        void RemoveTimingGroup(TimingGroup group);
    }
}