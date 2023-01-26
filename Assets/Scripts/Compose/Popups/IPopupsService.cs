using UnityEngine;

namespace ArcCreate.Compose.Popups
{
    public interface IPopupsService
    {
        /// <summary>
        /// Open a color picker at the specified screen position.
        /// </summary>
        /// <param name="screenPosition">The screen position to open the picker at.</param>
        /// <param name="defaultColor">The color value to initialize the picker with.</param>
        /// <returns>A color picker instance.</returns>
        ColorPickerWindow OpenColorPicker(Vector2 screenPosition, Color defaultColor);

        void CreateTextDialog(string title, string content, params ButtonSetting[] buttonSettings);

        void Notify(Severity severity, string content);

        /// <summary>
        /// You have the geniuses at arcthesia to blame for this.
        /// </summary>
        void PlayVineBoom();
    }
}