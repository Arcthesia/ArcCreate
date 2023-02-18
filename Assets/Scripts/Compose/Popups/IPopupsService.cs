using ArcCreate.Gameplay.Data;
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

        /// <summary>
        /// Open an arc type picker at the specified screen position.
        /// </summary>
        /// <param name="screenPosition">The screen position to open the picker at.</param>
        /// <param name="defaultType">The type value to initialize the picker with.</param>
        /// <param name="caller">The object that opened this window.</param>
        /// <returns>An arc type picker instance.</returns>
        ArcTypePickerWindow OpenArcTypePicker(Vector2 screenPosition, ArcLineType? defaultType, object caller);

        /// <summary>
        /// Open an arc color id picker at the specified screen position.
        /// </summary>
        /// <param name="screenPosition">The screen position to open the picker at.</param>
        /// <param name="defaultColor">The color id to initialize the picker with.</param>
        /// <param name="caller">The object that opened this window.</param>
        /// <returns>An arc type picker instance.</returns>
        ArcColorPickerWindow OpenArcColorPicker(Vector2 screenPosition, int? defaultColor, object caller);

        /// <summary>
        /// Open a timing group picker at the specified screen position.
        /// </summary>
        /// <param name="screenPosition">The screen position to open the picker at.</param>
        /// <param name="defaultTimingGroup">The timing group to initialize the picker with.</param>
        /// <param name="caller">The object that opened this window.</param>
        /// <returns>An arc type picker instance.</returns>
        TimingGroupPicker OpenTimingGroupPicker(Vector2 screenPosition, int? defaultTimingGroup, object caller);

        /// <summary>
        /// Open a text dialog.
        /// </summary>
        /// <param name="title">The dialog's title.</param>
        /// <param name="content">The dialog's content.</param>
        /// <param name="buttonSettings">List of buttons to display.</param>
        void CreateTextDialog(string title, string content, params ButtonSetting[] buttonSettings);

        /// <summary>
        /// Display text on the notification bar.
        /// </summary>
        /// <param name="severity">Severity of the notification. Affects the background color of the bar.</param>
        /// <param name="content">The text content to display.</param>
        void Notify(Severity severity, string content);

        /// <summary>
        /// You have the geniuses at arcthesia to blame for this.
        /// </summary>
        void PlayVineBoom();
    }
}