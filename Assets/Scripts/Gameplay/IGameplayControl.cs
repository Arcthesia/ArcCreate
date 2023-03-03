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
        bool IsLoaded { get; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to let this module update InputSystem.
        /// Set this to true if no other scene will update InputSystem themselves.
        /// </summary>
        bool ShouldUpdateInputSystem { get; set; }

        /// <summary>
        /// Gets the controller for chart related functionality.
        /// </summary>
        IChartControl Chart { get; }

        /// <summary>
        /// Gets the controller for skin related functionality.
        /// </summary>
        ISkinControl Skin { get; }

        /// <summary>
        /// Gets the controller for audio related functionality.
        /// </summary>
        IAudioControl Audio { get; }

        /// <summary>
        /// Gets the controller for camera related functionality.
        /// </summary>
        ICameraControl Camera { get; }

        /// <summary>
        /// Gets the controller for scenecontrol related functionality.
        /// </summary>
        IScenecontrolControl Scenecontrol { get; }

        /// <summary>
        /// Set the rectangle boundary to render this scene's camera onto.
        /// </summary>
        /// <param name="rect">The rect boundary, normalized.</param>
        void SetCameraViewportRect(Rect rect);

        /// <summary>
        /// Set whether or not to enable the cameras rendering this scene.
        /// </summary>
        /// <param name="enable">Whether or not to enable the cameras.</param>
        void SetCameraEnabled(bool enable);

        /// <summary>
        /// Sets whether or not to display arc debug information.
        /// </summary>
        /// <param name="enable">Whether or not to display arc debug.</param>
        void SetEnableArcDebug(bool enable);
    }
}