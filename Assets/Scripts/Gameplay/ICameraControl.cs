using UnityEngine;

namespace ArcCreate.Gameplay
{
    public interface ICameraControl
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not the gameplay camera is in a custom position/rotation.
        /// </summary>
        /// <value>Whether or not the gameplay camera is in a custom position/rotation.</value>
        bool IsEditorCamera { get; set; }

        /// <summary>
        /// Gets or sets the custom camera position.
        /// </summary>
        /// <value>The position vector value.</value>
        Vector3 EditorCameraPosition { get; set; }

        /// <summary>
        /// Gets or sets the custom camera rotation.
        /// </summary>
        /// <value>The rotation vector value.</value>
        Vector3 EditorCameraRotation { get; set; }

        /// <summary>
        /// Gets the gameplay camera of the scene.
        /// </summary>
        /// <value>The gameplay camera.</value>
        Camera GameplayCamera { get; }
    }
}