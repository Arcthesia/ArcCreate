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

        /// <summary>
        /// Get the timing group from a timing group number, and create new default timing groups if it does not exist yet.
        /// </summary>
        /// <param name="tg">The timing group number.</param>
        /// <returns>The timing group of that number.</returns>
        TimingGroup GetTimingGroup(int tg);

        /// <summary>
        /// Remove a timing group and all of its events.
        /// </summary>
        /// <param name="group">The timing group to remove.</param>
        void RemoveTimingGroup(TimingGroup group);

        /// <summary>
        /// Get all notes of a type.
        /// </summary>
        /// <typeparam name="T">The event type.</typeparam>
        /// <returns>All notes of the specified type.</returns>
        IEnumerable<T> GetAll<T>()
            where T : ArcEvent;
    }
}