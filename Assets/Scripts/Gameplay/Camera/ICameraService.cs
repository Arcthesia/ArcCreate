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
    }
}