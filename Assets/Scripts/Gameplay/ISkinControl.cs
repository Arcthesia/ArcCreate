using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Gameplay
{
    /// <summary>
    /// Boundary interface provided for controlling skin display.
    /// </summary>
    public interface ISkinControl
    {
        Sprite BackgroundTexture { get; set; }

        string VideoBackgroundUrl { get; set; }

        string NoteSkin { get; set; }

        string ParticleSkin { get; set; }

        string TrackSkin { get; set; }

        string SingleLineSkin { get; set; }

        string AccentSkin { get; }
    }
}