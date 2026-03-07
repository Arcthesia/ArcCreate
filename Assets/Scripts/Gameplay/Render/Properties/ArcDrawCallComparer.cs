using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcCreate.Gameplay.Render
{
    public class ArcDrawCallComparer : IComparer<ArcDrawCall>
    {
#if UNITY_EDITOR
        private static int ChainCompareDebug(ArcDrawCall a, ArcDrawCall b, params int[] compares)
        {
            foreach (int compare in compares)
            {
                if (compare != 0) return compare;
            }

            if (a.ColorId != b.ColorId) Debug.LogWarning("Cannot distinguish");
            return 0;
        }
#endif

        private static int ChainCompare(params int[] compares) => compares.FirstOrDefault(x => x != 0);

        private static int ByColorId(ArcDrawCall a, ArcDrawCall b) =>
            a.ColorId.CompareTo(b.ColorId);

        private static int ByDepth(ArcDrawCall a, ArcDrawCall b) =>
            a.Depth.CompareTo(b.Depth);

        private static int ByYPos(ArcDrawCall a, ArcDrawCall b) =>
            a.ArcStartPos.y.CompareTo(b.ArcStartPos.y);

        private static int ByYEndPos(ArcDrawCall a, ArcDrawCall b) =>
            a.ArcEndPos.y.CompareTo(b.ArcEndPos.y);

        private static int ByXPosAbs(ArcDrawCall a, ArcDrawCall b) =>
            Mathf.Abs(a.ArcStartPos.x - .5f).CompareTo(Mathf.Abs(b.ArcStartPos.x - .5f));

        private static int ByXEndPosAbs(ArcDrawCall a, ArcDrawCall b) =>
            Mathf.Abs(a.ArcEndPos.x - .5f).CompareTo(Mathf.Abs(b.ArcEndPos.x - .5f));

        private static int ByArcTiming(ArcDrawCall a, ArcDrawCall b) =>
            a.TimingStartEnd.x.CompareTo(b.TimingStartEnd.x);

        private static int ByArcEndTiming(ArcDrawCall a, ArcDrawCall b) =>
            a.TimingStartEnd.y.CompareTo(b.TimingStartEnd.y);

        public int Compare(ArcDrawCall a, ArcDrawCall b) =>
            ChainCompare(
                ByYPos(a, b),
                ByYEndPos(a, b),
                ByColorId(a, b),
                -ByXPosAbs(a, b),
                -ByXEndPosAbs(a, b),
                -ByArcTiming(a, b),
                -ByArcEndTiming(a, b),
                -ByDepth(a, b)
            );
    }
}