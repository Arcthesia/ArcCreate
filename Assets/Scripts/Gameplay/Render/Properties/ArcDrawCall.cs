using UnityEngine;

namespace ArcCreate.Gameplay.Render
{
    public struct ArcDrawCall
    {
        public Matrix4x4 Matrix;
        public Vector4 Color;
        public Vector4 Properties;
        public int ColorId;
        public float Depth;
        public Vector2 ArcStartPos;
        public Vector2 ArcEndPos;
        public Vector2Int TimingStartEnd;
    }
}