using UnityEngine;

namespace ArcCreate.Gameplay
{
    public interface ICameraControl
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not the gameplay camera is in a custom position/rotation.
        /// </summary>
        bool IsEditorCamera { get; set; }

        /// <summary>
        /// Gets or sets the custom camera position.
        /// </summary>
        Vector3 EditorCameraPosition { get; set; }

        /// <summary>
        /// Gets or sets the custom camera rotation.
        /// </summary>
        Vector3 EditorCameraRotation { get; set; }

        /// <summary>
        /// Gets the gameplay camera of the scene.
        /// </summary>
        Camera GameplayCamera { get; }
    }
}