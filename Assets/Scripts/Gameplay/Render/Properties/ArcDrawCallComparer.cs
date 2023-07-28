using System.Collections.Generic;

namespace ArcCreate.Gameplay.Render
{
    public class ArcDrawCallComparer : IComparer<ArcDrawCall>
    {
        public int Compare(ArcDrawCall x, ArcDrawCall y)
        {
            return -x.Depth.CompareTo(y.Depth);
        }
    }
}