using System.Collections.Generic;

namespace ArcCreate.Gameplay.Chart
{
    public interface IBeatlineGenerator
    {
        IEnumerable<Beatline> Generate(TimingGroup tg, int audioLength);
    }
}