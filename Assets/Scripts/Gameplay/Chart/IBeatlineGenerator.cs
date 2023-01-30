using System.Collections.Generic;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Chart
{
    public interface IBeatlineGenerator
    {
        IEnumerable<Beatline> Generate(TimingGroup tg);
    }
}