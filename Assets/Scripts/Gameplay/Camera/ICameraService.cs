using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay.GameplayCamera
{
    /// <summary>
    /// Services for controlling the main gameplay camera.
    /// </summary>
    public interface ICameraService : ICameraControl
    {
        /// <summary>
        /// Gets the list of all camera events.
        /// </summary>
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
        /// Find all scenecontrol events with matching timing.
        /// </summary>
        /// <param name="from">The query timing value range's lower end.</param>
        /// <param name="to">The query timing value range's upper end.</param>
        /// <returns>All matching scenecontrol events.</returns>
        IEnumerable<CameraEvent> FindByTiming(int from, int to);

        /// <summary>
        /// Find all camera events bounded by provided timing range.
        /// </summary>
        /// <param name="from">The timing range's lower bound.</param>
        /// <param name="to">The timing range's upper bound.</param>
        /// <param name="overlapCompletely">Whether to only query for notes that overlap with the range completely.</param>
        /// <returns>All matching camera events.</returns>
        IEnumerable<CameraEvent> FindWithinRange(int from, int to, bool overlapCompletely = true);

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

        /// <summary>
        /// Clear all events.
        /// </summary>
        void Clear();

        void SetPropertiesExternal(float fieldOfView, float tiltFactor);

        void SetTransformExternal(Vector3 translation, Quaternion rotation);

        float CalculateDepthSquared(Vector3 vector3);
    }
}