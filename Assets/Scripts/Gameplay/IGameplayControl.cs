using UnityEngine;

namespace ArcCreate.Gameplay
{
    /// <summary>
    /// Boundary interface of Gameplay module.
    /// </summary>
    public interface IGameplayControl
    {
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
        /// Set the target texture to render the gameplay scene to.
        /// Value of null will set target to the whole screen.
        /// </summary>
        /// <param name="renderTexture">The target render texture.</param>
        void SetTargetRenderTexture(RenderTexture renderTexture);

        /// <summary>
        /// Apply aspect ratio to the camera.
        /// </summary>
        /// <param name="aspectRatio">The aspect ratio.</param>
        void ApplyAspect(float aspectRatio);
    }
}