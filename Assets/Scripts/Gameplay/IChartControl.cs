using System.Collections.Generic;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay
{
    /// <summary>
    /// Boundary interface provided for controlling chart elements.
    /// </summary>
    public interface IChartControl
    {
        int Timing { get; set; }

        int AudioOffset { get; set; }

        float TimingPointDensityFactor { get; set; }

        AudioClip AudioClip { get; set; }

        /// <summary>
        /// Set the chart file for this system.
        /// </summary>
        /// <param name="reader">The chart reader defining the chart.</param>
        void SetChart(ChartReader reader);

        /// <summary>
        /// Add events to the currently playing chart.
        /// </summary>
        /// <param name="events">The events to add.</param>
        void Add(IEnumerable<ArcEvent> events);

        /// <summary>
        /// Remove events from the currently playing chart.
        /// </summary>
        /// <param name="events">The events to remove.</param>
        void Remove(IEnumerable<ArcEvent> events);

        /// <summary>
        /// Notify the system that these events have had their properties changed.
        /// This will force the system to recalculate the notes' floor position, relationship with other notes, etc.
        /// </summary>
        /// <param name="events">The events that has been changed.</param>
        void NotifyChange(IEnumerable<ArcEvent> events);
    }
}