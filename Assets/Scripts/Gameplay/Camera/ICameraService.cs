using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay.GameplayCamera
{
    /// <summary>
    /// Services for controlling the main gameplay camera.
    /// </summary>
    public interface ICameraService
    {
        /// <summary>
        /// Gets the gameplay camera.
        /// </summary>
        /// <value>The gameplay camera.</value>
        Camera GameplayCamera { get; }

        /// <summary>
        /// Gets the list of all camera events.
        /// </summary>
        /// <value>The list of camera events.</value>
        List<CameraEvent> Events { get; }

        /// <summary>
        /// Load a list of camera events.
        /// </summary>
        /// <param name="cameras">The list of events.</param>
        void Load(List<CameraEvent> cameras);

        /// <summary>
        /// Update the camera's state.
        /// </summary>
        /// <param name="currentTiming">The current audio timing.</param>
        void UpdateCamera(int currentTiming);

        /// <summary>
        /// Add an arc's world x position to consider for camera tilting.
        /// </summary>
        /// <param name="arcWorldX">The current worldX position of the arc.</param>
        void AddTiltToCamera(float arcWorldX);

        /// <summary>
        /// Find all camera events with matching timing.
        /// </summary>
        /// <param name="timing">The query timing value.</param>
        /// <returns>All matching camera events.</returns>
        IEnumerable<CameraEvent> FindByTiming(int timing);

        /// <summary>
        /// Find all camera events bounded by provided timing range.
        /// </summary>
        /// <param name="from">The timing range's lower bound.</param>
        /// <param name="to">The timing range's upper bound.</param>
        /// <returns>All matching camera events.</returns>
        IEnumerable<CameraEvent> FindWithinRange(int from, int to);

        /// <summary>
        /// Add a collection of camera events.
        /// </summary>
        /// <param name="events">The events collection.</param>
        void Add(IEnumerable<CameraEvent> events);

        /// <summary>
        /// Remove a collection of camera events.
        /// </summary>
        /// <param name="events">The events collection.</param>
        void Remove(IEnumerable<CameraEvent> events);

        /// <summary>
        /// Notify a collection of camera events have had their values changed.
        /// </summary>
        /// <param name="events">The events collection.</param>
        void Change(IEnumerable<CameraEvent> events);
    }
}