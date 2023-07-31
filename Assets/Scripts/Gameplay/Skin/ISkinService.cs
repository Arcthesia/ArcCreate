using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.Gameplay.Skin
{
    /// <summary>
    /// Interface for providing skins related services to internal (Gameplay) classes.
    /// </summary>
    public interface ISkinService : ISkinControl
    {
        /// <summary>
        /// Gets the combo color of the current skin.
        /// </summary>
        Color ComboColor { get; }

        /// <summary>
        /// Gets the default background according to current skin value.
        /// </summary>
        Sprite DefaultBackground { get; }

        /// <summary>
        /// Get the track skin.
        /// </summary>
        /// <param name="name">The track skin option name.</param>
        /// <returns>A tuple of:<br/>
        /// lane: The main lane's sprite.<br/>
        /// extraLane: The extra lane's material.
        /// </returns>
        (Sprite lane, Sprite extraLane) GetTrackSprite(string name);

        /// <summary>
        /// Apply red arc value to a color.
        /// </summary>
        /// <param name="color">The color id.</param>
        /// <param name="value">The red arc value from 0 to 1.</param>
        void ApplyRedArcValue(int color, float value);

        (Color high, Color low) GetArcColor(int colorId);

        (Color color1, Color color2) GetArcParticleColor(int colorId);

        void SetVideoBackground(string path, bool isUri);
    }
}