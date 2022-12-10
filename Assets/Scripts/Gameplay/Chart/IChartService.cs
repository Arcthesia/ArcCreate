using System.Collections.Generic;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Chart
{
    /// <summary>
    /// Interface for providing chart control services to internal (Gameplay) classes.
    /// </summary>
    public interface IChartService
    {
        /// <summary>
        /// Reload skin for all notes in the chart.
        /// </summary>
        void ReloadSkin();

        /// <summary>
        /// Reset judgement state.
        /// </summary>
        void ResetJudge();

        /// <summary>
        /// Get all notes of a type.
        /// </summary>
        /// <typeparam name="T">The event type.</typeparam>
        /// <returns>All notes of the specified type.</returns>
        IEnumerable<T> GetAll<T>()
            where T : ArcEvent;

        /// <summary>
        /// Find all events that have matching timing value.
        /// </summary>
        /// <param name="timing">The query timing value.</param>
        /// <typeparam name="T">Event type to search for.</typeparam>
        /// <returns>All events with matching timing value.</returns>
        IEnumerable<T> FindByTiming<T>(int timing)
            where T : ArcEvent;

        /// <summary>
        /// Find all long notes that have matching end timing value.
        /// </summary>
        /// <param name="endTiming">The query end timing value.</param>
        /// <typeparam name="T">Long note type to search for.</typeparam>
        /// <returns>All long notes with matching end timing value.</returns>
        IEnumerable<T> FindByEndTiming<T>(int endTiming)
            where T : LongNote;

        /// <summary>
        /// Find all events that are bounded by the provided timing range.
        /// I.e note.Timing >= from && note.EndTiming. <= to.
        /// </summary>
        /// <param name="from">The query timing lower range.</param>
        /// <param name="to">The query timing upper range.</param>
        /// <typeparam name="T">Event type to search for.</typeparam>
        /// <returns>All events with matching timing value.</returns>
        IEnumerable<T> FindEventsWithinRange<T>(int from, int to)
            where T : ArcEvent;

        /// <summary>
        /// Load a chart.
        /// </summary>
        /// <param name="chart">Chart to load.</param>
        void LoadChart(ArcChart chart);

        /// <summary>
        /// Clear the chart.
        /// </summary>
        void Clear();

        /// <summary>
        /// Add a collection of events to the current chart.
        /// </summary>
        /// <param name="e">The events to add.</param>
        void AddEvents(IEnumerable<ArcEvent> e);

        /// <summary>
        /// Remove a collection of events to the current chart.
        /// </summary>
        /// <param name="e">The events to remove.</param>
        void RemoveEvents(IEnumerable<ArcEvent> e);

        /// <summary>
        /// Notify that a collection of events have had their properties changed.
        /// </summary>
        /// <param name="e">The events that has changed.</param>
        void UpdateEvents(IEnumerable<ArcEvent> e);

        /// <summary>
        /// Get the timing group from a timing group number, and create new default timing groups if it does not exist yet.
        /// </summary>
        /// <param name="tg">The timing group number.</param>
        /// <returns>The timing group of that number.</returns>
        TimingGroup GetTimingGroup(int tg);

        /// <summary>
        /// Update all notes in the chart.
        /// </summary>
        /// <param name="currentTiming">The current audio timing.</param>
        void UpdateChart(int currentTiming);
    }
}