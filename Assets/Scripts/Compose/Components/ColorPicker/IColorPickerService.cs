using System;
using UnityEngine;

namespace ArcCreate.Compose.Components
{
    public interface IColorPickerService
    {
        /// <summary>
        /// Gets or sets the callback after the color value has changed.
        /// </summary>
        /// <value>The callback.</value>
        Action<Color> OnColorChanged { get; set; }

        /// <summary>
        /// Open the picker at the specified screen position.
        /// </summary>
        /// <param name="screenPosition">The screen position to open the picker at.</param>
        /// <param name="defaultColor">The color value to initialize the picker with.</param>
        void OpenAt(Vector2 screenPosition, Color defaultColor);
    }
}