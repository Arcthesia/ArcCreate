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
        /// <param name="isSFX">is SFX</param>
        void PlayTapParticle(Vector3 worldPosition, JudgementResult result, bool isSFX);

        /// <summary>
        /// Play a text (e.g "Perfect") particle. The coordinate will be translated to screen-space coordinate.
        /// Additionally display the "EARLY" / "LATE" text if judgement matches, or if DisplayMsDifference is on, will display the ms offset.
        /// </summary>
        /// <param name="worldPosition">The world-space coordinate.</param>
        /// <param name="result">Judgement to play.</param>
        /// <param name="offset">Offset from theoretical timing.</param>
        void PlayTextParticle(Vector3 worldPosition, JudgementResult result, Option<int> offset);

        /// <summary>
        /// Play an arc note particle. The coordinate will be translated to screen-space coordinate.
        /// </summary>
        /// <param name="colorId">The color id of the arc.</param>
        /// <param name="reference">The reference. The same particle object will be used for the same reference.</param>
        /// <param name="worldPosition">The world-space coordinate.</param>
        void PlayArcParticle(int colorId, LongNote reference, Vector3 worldPosition);

        /// <summary>
        /// Play a hold note particle. The coordinate will be translated to screen-space coordinate.
        /// </summary>
        /// <param name="reference">The reference. The same particle object will be used for the same reference.</param>
        /// <param name="worldPosition">The world-space coordinate.</param>
        void PlayHoldParticle(LongNote reference, Vector3 worldPosition);

        /// <summary>
        /// Change tap particle skin.
        /// </summary>
        /// <param name="particleTexture">The tap texture.</param>
        void SetTapParticleSkin(Texture particleTexture);

        /// <summary>
        /// Change SFX tap particle skin.
        /// </summary>
        /// <param name="particleTexture">The tap texture.</param>
        void SetTapSfxParticleSkin(Texture sfxParticleTexture);

        /// <summary>
        /// Change hold and arc note's particle skin.
        /// </summary>
        /// <param name="colorMin">The min color of the color range.</param>
        /// <param name="colorMax">The max color of the color range.</param>
        /// <param name="fromGradient">The first color-over-time gradient.</param>
        /// <param name="toGradient">The second color-over-time gradient.</param>
        /// <param name="colorGrid">The color of the grid element.</param>
        void SetHoldParticleSkin(Color colorMin, Color colorMax, Gradient fromGradient, Gradient toGradient, Color colorGrid);
    }
}