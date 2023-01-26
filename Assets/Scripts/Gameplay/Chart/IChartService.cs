using System.Collections.Generic;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Chart
{
    /// <summary>
    /// Interface for providing chart control services to internal (Gameplay) classes.
    /// </summary>
    public interface IChartService : IChartControl
    {
        /// <summary>
        /// Gets a value indicating whether a chart has been loaded.
        /// </summary>
        /// <value>The boolean value.</value>
        bool IsLoaded { get; }

        /// <summary>
        /// Reload skin for all notes in the chart.
        /// </summary>
        void ReloadSkin();

        /// <summary>
        /// Reset judgement state.
        /// </summary>
        void ResetJudge();

        /// <summary>
        /// Reload beatline display.
        /// </summary>
        void ReloadBeatline();

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
        void LoadChart(ChartReader chart);

        /// <summary>
        /// Clear the chart.
        /// </summary>
        void Clear();

        /// <summary>
        /// Update all notes in the chart.
        /// </summary>
        /// <param name="currentTiming">The current audio timing.</param>
        void UpdateChart(int currentTiming);
    }
}