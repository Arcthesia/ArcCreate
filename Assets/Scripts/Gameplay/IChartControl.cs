using System.Collections.Generic;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay
{
    public interface IChartControl
    {
        /// <summary>
        /// Gets or sets the audio timing.
        /// Setting the timing will cause score to reset to 0.
        /// </summary>
        /// <value>The audio timing.</value>
        int Timing { get; set; }

        /// <summary>
        /// Gets or sets the audio offset.
        /// Setting the timing will cause score to reset to 0.
        /// </summary>
        /// <value>The audio offset.</value>
        int AudioOffset { get; set; }

        /// <summary>
        /// Gets or sets the timing point density factor.
        /// Setting the timing will cause score to reset to 0.
        /// </summary>
        /// <value>The timing point density factor value.</value>
        float TimingPointDensityFactor { get; set; }

        /// <summary>
        /// Gets or sets the audio clip to play.
        /// </summary>
        /// <value>The audio clip.</value>
        AudioClip AudioClip { get; set; }

        /// <summary>
        /// Set the chart file for this system.
        /// </summary>
        /// <param name="reader">The chart reader defining the chart.</param>
        void LoadChart(ChartReader reader);

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

        void Play();
    }
}