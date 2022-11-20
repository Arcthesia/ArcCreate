using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay.Skin
{
    /// <summary>
    /// Interface for providing skins related services to internal (Gameplay) classes.
    /// </summary>
    public interface ISkinService
    {
        /// <summary>
        /// Get the skin for an arc note.
        /// </summary>
        /// <param name="note">The note to get the skin for.</param>
        /// <returns>A tuple of:<br/>
        /// normal: The default material.<br/>
        /// highlight: The highlighted material.<br/>
        /// arcCap: The sprite for its arc cap.<br/>
        /// psMin: The minimum color for particle system.<br/>
        /// psMax: The maximum color for particle system.
        /// </returns>
        (Material normal, Material highlight, Sprite arcCap, Color psMin, Color psMax) GetArcSkin(Arc note);

        /// <summary>
        /// Get the skin for a tap note.
        /// </summary>
        /// <param name="note">The note to get the skin for.</param>
        /// <returns>The correct tap note sprite.</returns>
        Sprite GetTapSkin(Tap note);

        /// <summary>
        /// Get the skin for a hold note.
        /// </summary>
        /// <param name="note">The note to get the skin for.</param>
        /// <returns>A tuple of:<br/>
        /// normal: The normal sprite.<br/>
        /// highlight: The highlighted sprite.<br/>
        /// </returns>
        (Sprite normal, Sprite highlight) GetHoldSkin(Hold note);

        /// <summary>
        /// Get the skin for a hold note.
        /// </summary>
        /// <param name="note">The note to get the skin for.</param>
        /// <returns>A tuple of:<br/>
        /// mesh: The note's mesh.<br/>
        /// material: The note's material.
        /// </returns>
        (Mesh mesh, Material material) GetArcTapSkin(ArcTap note);

        /// <summary>
        /// Get the track skin.
        /// </summary>
        /// <param name="name">The track skin option name.</param>
        /// <returns>A tuple of:<br/>
        /// lane: The main lane's sprite.<br/>
        /// extraLane: The extra lane's material.
        /// </returns>
        (Sprite lane, Sprite extraLane) GetTrackSprite(string name);
    }
}