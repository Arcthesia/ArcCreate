using ArcCreate.Gameplay.Data;
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
        /// <param name="worldPosition">The world-space coordinate.</param>
        /// <param name="result">Judgement to play.</param>
        void PlayTapParticle(Vector3 worldPosition, JudgementResult result);

        /// <summary>
        /// Play a text (e.g "PURE") particle. The coordinate will be translated to screen-space coordinate.
        /// Additionally display the "EARLY" / "LATE" text if judgement matches.
        /// </summary>
        /// <param name="worldPosition">The world-space coordinate.</param>
        /// <param name="result">Judgement to play.</param>
        void PlayTextParticle(Vector3 worldPosition, JudgementResult result);

        /// <summary>
        /// Play a long note particle. The coordinate will be translated to screen-space coordinate.
        /// </summary>
        /// <param name="reference">The reference. The same particle object will be used for the same reference.</param>
        /// <param name="worldPosition">The world-space coordinate.</param>
        void PlayLongParticle(LongNote reference, Vector3 worldPosition);

        /// <summary>
        /// Change tap particle skin.
        /// </summary>
        /// <param name="particleTexture">The tap texture.</param>
        void SetTapParticleSkin(Texture particleTexture);

        /// <summary>
        /// Change hold and arc note's particle skin.
        /// </summary>
        /// <param name="colorMin">The min color of the color range.</param>
        /// <param name="colorMax">The max color of the color range.</param>
        void SetLongParticleSkin(Color colorMin, Color colorMax);
    }
}