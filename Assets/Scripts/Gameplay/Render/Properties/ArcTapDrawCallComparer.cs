using System.Collections.Generic;

namespace ArcCreate.Gameplay.Render
{
    public class ArcTapDrawCallComparer : IComparer<ArcTapDrawCall>
    {
        public int Compare(ArcTapDrawCall x, ArcTapDrawCall y)
        {
            return x.Depth.CompareTo(y.Depth);
        }
    }
}