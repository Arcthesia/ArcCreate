using ArcCreate.Gameplay.Judgement;
using UnityEngine;

namespace ArcCreate.Gameplay.Particle
{
    /// <summary>
    /// Interface for providing hit effects services to internal (Gameplay) classes.
    /// </summary>
    public interface IParticleService
    {
        /// <summary>
        /// Update the state of particles.
        /// </summary>
        void UpdateParticles();

        /// <summary>
        /// Play a tap particle. The coordinate will be translated to screen-space coordinate.
        /// </summary>
        /// <param name="vector3">The world-space coordinate.</param>
        /// <param name="result">Judgement to play.</param>
        void PlayTapParticle(Vector3 vector3, JudgementResult result);

        /// <summary>
        /// Play a text (e.g "PURE") particle. The coordinate will be translated to screen-space coordinate.
        /// Additionally display the "EARLY" / "LATE" text if judgement matches.
        /// </summary>
        /// <param name="vector3">The world-space coordinate.</param>
        /// <param name="result">Judgement to play.</param>
        void PlayTextParticle(Vector3 vector3, JudgementResult result);

        /// <summary>
        /// Change tap particle skin.
        /// </summary>
        /// <param name="particleTexture">The tap texture.</param>
        void SetTapParticleSkin(Texture particleTexture);
    }
}