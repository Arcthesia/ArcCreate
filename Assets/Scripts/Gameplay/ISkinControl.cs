using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Gameplay
{
    public interface ISkinControl
    {
        Color DefaultTraceColor { get; }

        Color DefaultShadowColor { get; }

        List<Color> DefaultArcColors { get; }

        List<Color> DefaultArcLowColors { get; }

        Color UnknownArcColor { get; }

        Color UnknownArcLowColor { get; }

        /// <summary>
        /// Gets or sets the background image's sprite.
        /// </summary>
        /// <value>The image's sprite.</value>
        Sprite BackgroundSprite { get; set; }

        /// <summary>
        /// Gets or sets the url to be played by video background renderer.
        /// Setting it to null or empty string will disable the renderer.
        /// </summary>
        /// <value>The video background url.</value>
        string VideoBackgroundUrl { get; set; }

        /// <summary>
        /// Gets or sets the alignment skin option.
        /// If the name does not match any available option, a default option will be used.
        /// </summary>
        /// <value>The name of the skin option.</value>
        string AlignmentSkin { get; set; }

        /// <summary>
        /// Gets or sets the note skin option.
        /// If the name does not match any available option,
        /// the option provided by current Alignment skin will be used.
        /// </summary>
        /// <value>The name of the skin option.</value>
        string NoteSkin { get; set; }

        /// <summary>
        /// Gets or sets the particle skin option.
        /// If the name does not match any available option,
        /// the option provided by current Alignment skin will be used.
        /// </summary>
        /// <value>The name of the skin option.</value>
        string ParticleSkin { get; set; }

        /// <summary>
        /// Gets or sets the track skin option.
        /// If the name does not match any available option,
        /// the option provided by current Alignment skin will be used.
        /// </summary>
        /// <value>The name of the skin option.</value>
        string TrackSkin { get; set; }

        /// <summary>
        /// Gets or sets the single line skin option.
        /// If the name does not match any available option,
        /// the option provided by current Alignment skin will be used.
        /// </summary>
        /// <value>The name of the skin option.</value>
        string SingleLineSkin { get; set; }

        /// <summary>
        /// Gets or sets the accent skin option.
        /// If the name does not match any available option,
        /// the option provided by current Alignment skin will be used.
        /// </summary>
        /// <value>The name of the skin option.</value>
        string AccentSkin { get; set; }

        /// <summary>
        /// Sets the trace color.
        /// </summary>
        /// <param name="color">The trace body color.</param>
        /// <param name="shadow">The trace shadow color.</param>
        void SetTraceColor(Color color, Color shadow);

        /// <summary>
        /// Sets the arc color.
        /// </summary>
        /// <param name="arcs">The list of arc colors at y=1.</param>
        /// <param name="arcLows">The list of arc colors at y=0.</param>
        /// <param name="shadow">The arc shadow color.</param>
        void SetArcColors(List<Color> arcs, List<Color> arcLows, Color shadow);

        /// <summary>
        /// Resets trace colors to default.
        /// </summary>
        void ResetTraceColors();

        /// <summary>
        /// Reset arc colors to default.
        /// </summary>
        void ResetArcColors();
    }
}