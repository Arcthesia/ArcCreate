using ArcCreate.Gameplay.Chart;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public struct ArcSegmentData
    {
        public int Timing;

        public int EndTiming;

        public Vector3 StartPosition;

        public Vector3 EndPosition;

        public double FloorPosition;

        public double EndFloorPosition;

        public float From;

        public float CalculateZPos(double currentFloorPosition)
            => ArcFormula.FloorPositionToZ(FloorPosition - currentFloorPosition);

        public float CalculateEndZPos(double currentFloorPosition)
            => ArcFormula.FloorPositionToZ(EndFloorPosition - currentFloorPosition);

        public (Matrix4x4 body, Matrix4x4 shadow, Vector4 cornerOffset) GetMatrices(double floorPosition, Vector3 fallDirection, float baseZ, float baseY)
        {
            float startZ = ArcFormula.FloorPositionToZ(FloorPosition - floorPosition);
            float endZ = ArcFormula.FloorPositionToZ(EndFloorPosition - floorPosition);
            Vector3 startPos = StartPosition + ((startZ - baseZ) * fallDirection);
            Vector3 endPos = EndPosition + ((endZ - baseZ) * fallDirection);
            startPos = ((endPos - startPos) * From) + startPos;
            Vector3 dir = endPos - startPos;

            Matrix4x4 bodyMatrix = new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(dir.x, dir.y, dir.z, 0),
                new Vector4(startPos.x, startPos.y, startPos.z, 1));

            Matrix4x4 shadowMatrix = new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(dir.x, 0, dir.z, 0),
                new Vector4(startPos.x, -baseY, startPos.z, 1));

            return (bodyMatrix, shadowMatrix, Vector4.zero);
        }

        public (Matrix4x4 body, Matrix4x4 shadow, Vector4 cornerOffset)
            GetMatricesSlam(double floorPosition, Vector3 fallDirection, float baseZ, Vector3 basePos, TimingGroup group, Arc next, float offset)
        {
            if (From > 0)
            {
                return (Matrix4x4.zero, Matrix4x4.zero, Vector4.zero);
            }

            float startZ = ArcFormula.FloorPositionToZ(FloorPosition - floorPosition);
            Vector3 startPos = StartPosition + ((startZ - baseZ) * fallDirection);
            Vector3 endPos = EndPosition + ((startZ - baseZ) * fallDirection);
            Vector3 dir = endPos - startPos;

            Matrix4x4 bodyMatrix = new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(dir.x, dir.y, dir.z, 0),
                new Vector4(startPos.x, startPos.y, startPos.z, 1));

            // Accounting for angled arcs being less wide than straight arcs
            float zLength = offset * 2;
            if (next != null && next.TryGetFirstSegement(out ArcSegmentData nextFirstSeg)
             && nextFirstSeg.EndTiming > nextFirstSeg.Timing)
            {
                float dz = Mathf.Abs(ArcFormula.FloorPositionToZ(nextFirstSeg.EndFloorPosition - nextFirstSeg.FloorPosition));
                float dx = Mathf.Abs(nextFirstSeg.StartPosition.x - nextFirstSeg.EndPosition.x);
                zLength = Mathf.Sqrt(4 * offset * offset * dz * dz / ((dz * dz) + (dx * dx)));
            }

            double fpOffset = ArcFormula.ZToFloorPosition(zLength);

            // Arc might go backward or forward in time, need to check both direction.
            int slamForwardTiming = group.GetTimingFromFloorPosition(FloorPosition + fpOffset);
            int slamBackwardTiming = group.GetTimingFromFloorPosition(FloorPosition - fpOffset);
            int endOfSlamTiming = Mathf.Max(slamForwardTiming, slamBackwardTiming);

            // Likely speed=0. Do not render shadow.
            if (endOfSlamTiming <= Timing)
            {
                return (bodyMatrix, Matrix4x4.zero, Vector4.zero);
            }

            bool isPositiveSpeed = slamForwardTiming > slamBackwardTiming;

            // Get the necessary coords.
            //         nf                                   ^ Z axis
            //    +    x     +flx - - - - - - - - +frx      |
            //    |    |     |       shadow       .         |
            //    | next arc |               pf   .         |
            //    +----x-----+nlx - -  +-----x----+nrx      |
            //         nn              | prev arc |         |
            //                         |     |    |         |
            //                         |     x    |         |
            //                               pn             |
            // ----------------------------------> X axis
            float pf = startPos.x;
            float nn = endPos.x;
            float nf = (next == null || next.EndTiming <= next.Timing) ? nn : next.WorldSegmentedXAt(endOfSlamTiming) - basePos.x;

            float nrx, nlx, frx, flx = 0;
            if (nn < pf)
            {
                nrx = pf + offset;
                nlx = nn + offset;
                flx = Mathf.Min(nf + offset, nrx);
                frx = Mathf.Max(nf + offset, nrx);
            }
            else
            {
                nlx = pf - offset;
                nrx = nn - offset;
                flx = Mathf.Min(nf - offset, nlx);
                frx = Mathf.Max(nf - offset, nlx);
            }

            zLength *= isPositiveSpeed ? 1 : -1;
            float zn = startPos.z;
            float zf = zn + zLength;

            float width = (nrx - nlx) / (offset * 2);
            Matrix4x4 shadowMatrix = new Matrix4x4(
                new Vector4(width, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(0, 0, zLength, 0),
                new Vector4((nlx + nrx) / 2, -basePos.y, 0, 1));

            Vector4 cornerOffset = new Vector4(0, frx - nrx, 0, flx - nlx) / width;
            return (bodyMatrix, shadowMatrix, cornerOffset);
        }
    }
}