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

        public (Matrix4x4 body, Matrix4x4 shadow) GetMatrices(double floorPosition, Vector3 fallDirection, float baseZ, float baseY, Vector3 noteScale)
        {
            float startZ = ArcFormula.FloorPositionToZ(FloorPosition - floorPosition);
            float endZ = ArcFormula.FloorPositionToZ(EndFloorPosition - floorPosition);
            Vector3 startPos = StartPosition + ((startZ - baseZ) * fallDirection);
            Vector3 endPos = EndPosition + ((endZ - baseZ) * fallDirection);
            startPos = ((endPos - startPos) * From) + startPos;
            Vector3 dir = endPos - startPos;

            startPos.x /= noteScale.x;
            startPos.y /= noteScale.y;

            endPos.x /= noteScale.x;
            endPos.y /= noteScale.y;

            Matrix4x4 bodyMatrix = new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(dir.x / noteScale.x, dir.y / noteScale.y, dir.z / noteScale.z, 0),
                new Vector4(startPos.x, startPos.y, startPos.z, 1));

            Matrix4x4 shadowMatrix = new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(dir.x / noteScale.x, 0, dir.z / noteScale.z, 0),
                new Vector4(startPos.x, -baseY, startPos.z, 1));

            return (bodyMatrix, shadowMatrix);
        }
    }
}