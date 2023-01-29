using UnityEngine;

namespace ArcCreate.Gameplay
{
    /// <summary>
    /// Boundary interface of Gameplay module.
    /// </summary>
    public interface IGameplayControl
    {
        /// <summary>
        /// Gets a value indicating whether or not the gameplay scene has successfully loaded a chart.
        /// </summary>
        /// <value>Whether or not a chart has been loaded.</value>
        bool IsLoaded { get; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to let this module update InputSystem.
        /// Set this to true if no other scene will update InputSystem themselves.
        /// </summary>
        /// <value>Whether or not to update InputSystem.</value>
        bool ShouldUpdateInputSystem { get; set; }

        /// <summary>
        /// Gets the controller for chart related functionality.
        /// </summary>
        /// <value>The chart controller.</value>
        IChartControl Chart { get; }

        /// <summary>
        /// Gets the controller for skin related functionality.
        /// </summary>
        /// <value>The skin controller.</value>
        ISkinControl Skin { get; }

        /// <summary>
        /// Gets the controller for audio related functionality.
        /// </summary>
        /// <value>The audio controller.</value>
        IAudioControl Audio { get; }

        /// <summary>
        /// Gets the controller for camera related functionality.
        /// </summary>
        /// <value>The camera controller.</value>
        ICameraControl Camera { get; }

        /// <summary>
        /// Set the rectangle boundary to render this scene's camera onto.
        /// </summary>
        /// <param name="rect">The rect boundary, normalized.</param>
        void SetCameraViewportRect(Rect rect);
    }
}