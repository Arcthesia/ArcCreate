using UnityEngine;

namespace ArcCreate.Gameplay.Effect
{
    /// <summary>
    /// Interface for providing hit effects services to internal (Gameplay) classes.
    /// </summary>
    public interface IEffectService
    {
        void SetParticleSkin(Texture particleTexture);
    }
}