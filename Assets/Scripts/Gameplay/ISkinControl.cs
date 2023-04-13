using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay
{
    public interface ISkinControl
    {
        /// <summary>
        /// Gets the default trace color.
        /// </summary>
        Color DefaultTraceColor { get; }

        /// <summary>
        /// Gets the default shadow color.
        /// </summary>
        Color DefaultShadowColor { get; }

        /// <summary>
        /// Gets the default arc colors at y=1.
        /// </summary>
        List<Color> DefaultArcColors { get; }

        /// <summary>
        /// Gets the default arc colors at y=0.
        /// </summary>
        List<Color> DefaultArcLowColors { get; }

        /// <summary>
        /// Gets the arc colors at y=1 for arcs with undefined colors.
        /// </summary>
        Color UnknownArcColor { get; }

        /// <summary>
        /// Gets the arc colors at y=0 for arcs with undefined colors.
        /// </summary>
        Color UnknownArcLowColor { get; }

        /// <summary>
        /// Gets or sets the alignment skin option.
        /// If the name does not match any available option, a default option will be used.
        /// </summary>
        string AlignmentSkin { get; set; }

        /// <summary>
        /// Gets or sets the note skin option.
        /// If the name does not match any available option,
        /// the option provided by current Alignment skin will be used.
        /// </summary>
        string NoteSkin { get; set; }

        /// <summary>
        /// Gets or sets the particle skin option.
        /// If the name does not match any available option,
        /// the option provided by current Alignment skin will be used.
        /// </summary>
        string ParticleSkin { get; set; }

        /// <summary>
        /// Gets or sets the track skin option.
        /// If the name does not match any available option,
        /// the option provided by current Alignment skin will be used.
        /// </summary>
        string TrackSkin { get; set; }

        /// <summary>
        /// Gets or sets the single line skin option.
        /// If the name does not match any available option,
        /// the option provided by current Alignment skin will be used.
        /// </summary>
        string SingleLineSkin { get; set; }

        /// <summary>
        /// Gets or sets the accent skin option.
        /// If the name does not match any available option,
        /// the option provided by current Alignment skin will be used.
        /// </summary>
        string AccentSkin { get; set; }

        /// <summary>
        /// Sets the trace color.
        /// </summary>
        /// <param name="color">The trace body color.</param>
        void SetTraceColor(Color color);

        /// <summary>
        /// Sets the arc color.
        /// </summary>
        /// <param name="arcs">The list of arc colors at y=1.</param>
        /// <param name="arcLows">The list of arc colors at y=0.</param>
        void SetArcColors(List<Color> arcs, List<Color> arcLows);

        /// <summary>
        /// Sets the shadow color of traces and arcs.
        /// </summary>
        /// <param name="color">The shadow color.</param>
        void SetShadowColor(Color color);

        /// <summary>
        /// Resets trace colors to default.
        /// </summary>
        void ResetTraceColors();

        /// <summary>
        /// Reset arc colors to default.
        /// </summary>
        void ResetArcColors();

        /// <summary>
        /// Get the skin for an arc note.
        /// </summary>
        /// <param name="note">The note to get the skin for.</param>
        /// <returns>Arccap texture, height indicator color.</returns>
        (Texture arcCap, Color heightIndicatorColor) GetArcSkin(Arc note);

        /// <summary>
        /// Get the red arc value for an arc color id.
        /// </summary>
        /// <param name="color">The color id.</param>
        /// <returns>The red arc value which is bounded from 0 to 1.</returns>
        float GetRedArcValue(int color);

        /// <summary>
        /// Get the skin for a tap note.
        /// </summary>
        /// <param name="note">The note to get the skin for.</param>
        /// <returns>The tap note's texture and connection line color.</returns>
        (Texture texture, Color connectionLineColor) GetTapSkin(Tap note);

        /// <summary>
        /// Get the skin for a hold note.
        /// </summary>
        /// <param name="note">The note to get the skin for.</param>
        /// <returns>The texture skin.</returns>
        Texture GetHoldSkin(Hold note);

        /// <summary>
        /// Get the skin for a hold note.
        /// </summary>
        /// <param name="note">The note to get the skin for.</param>
        /// <returns>The arctap's texture.</returns>
        Texture GetArcTapSkin(ArcTap note);
    }
}