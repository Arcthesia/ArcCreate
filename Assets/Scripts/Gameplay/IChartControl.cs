using System.Collections.Generic;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay
{
    public interface IChartControl
    {
        /// <summary>
        /// Gets the list of timing groups in the current chart.
        /// </summary>
        List<TimingGroup> TimingGroups { get; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to recreate arc segments collider mesh when rebuilding.
        /// Useful for operations that rebuild the arcs every frame.
        /// </summary>
        bool EnableArcRebuildSegment { get; set; }

        int NoteCount { get; }

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
        /// Get the timing group from a timing group number, and create a new default timing group if it does not exist yet.
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
        /// Insert a timing group and all of its events. The group's Number property is taken into consideration when inserting.
        /// The group number is clamped to the range [1, current group count].
        /// </summary>
        /// <param name="group">The timing group to insert.</param>
        void InsertTimingGroup(TimingGroup group);

        /// <summary>
        /// Get all notes of a type.
        /// </summary>
        /// <typeparam name="T">The event type.</typeparam>
        /// <returns>All notes of the specified type.</returns>
        IEnumerable<T> GetAll<T>()
            where T : ArcEvent;

        /// <summary>
        /// Find all events that are bounded by the provided timing range.
        /// </summary>
        /// <param name="from">The query timing lower range.</param>
        /// <param name="to">The query timing upper range.</param>
        /// <param name="overlapCompletely">Whether to only query for notes that overlap with the range completely.</param>
        /// <typeparam name="T">Event type to search for.</typeparam>
        /// <returns>All events with matching timing value.</returns>
        IEnumerable<T> FindEventsWithinRange<T>(int from, int to, bool overlapCompletely = true)
            where T : ArcEvent;

        /// <summary>
        /// Get all notes being rendered. Calculation is done with z position only (no frustum culling).
        /// </summary>
        /// <returns>All notes that's rendered.</returns>
        IEnumerable<Note> GetRenderingNotes();

        /// <summary>
        /// Clear the currently playing chart.
        /// </summary>
        void Clear();
    }
}