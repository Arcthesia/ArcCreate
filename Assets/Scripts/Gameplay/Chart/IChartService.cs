using System;
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
        /// Find notes within the chart that has properties within the specified range.
        /// Note that note list should be sorted by the property.
        /// </summary>
        /// <param name="valueMin">The minimum value of the range.</param>
        /// <param name="valueMax">The maximum value of the range.</param>
        /// <param name="property">Function to extract the property from a note.</param>
        /// <typeparam name="T">The note type.</typeparam>
        /// <typeparam name="R">The property type.</typeparam>
        /// <returns>All notes within the chart that has properties within the range.</returns>
        List<T> Find<T, R>(R valueMin, R valueMax, Func<T, R> property)
            where T : Note
            where R : IComparable<R>;

        /// <summary>
        /// Get all notes of a type.
        /// </summary>
        /// <typeparam name="T">The event type.</typeparam>
        /// <returns>All notes of the specified type.</returns>
        IEnumerable<T> GetAll<T>()
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