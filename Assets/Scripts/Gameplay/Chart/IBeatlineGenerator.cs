using System.Collections.Generic;

namespace ArcCreate.Gameplay.Chart
{
    public interface IBeatlineGenerator
    {
        /// <summary>
        /// Generate the beatline from the timing list of the specified timing group.
        /// Beatlines generate until their timing exceeds the the provided audioLength.
        /// </summary>
        /// <param name="tg">The timing group.</param>
        /// <param name="audioLength">The audio's length in ms.</param>
        /// <returns>The list of beatline, setup with proper floor position values.</returns>
        IEnumerable<Beatline> Generate(TimingGroup tg, int audioLength);
    }
}