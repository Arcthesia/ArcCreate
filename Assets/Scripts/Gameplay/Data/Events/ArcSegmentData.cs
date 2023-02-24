using System;
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

        public (Matrix4x4 body, Matrix4x4 shadow) GetMatrices(double floorPosition, Vector3 fallDirection, float baseZ)
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
                new Vector4(startPos.x, 0, startPos.z, 1));

            return (bodyMatrix, shadowMatrix);
        }
    }
}