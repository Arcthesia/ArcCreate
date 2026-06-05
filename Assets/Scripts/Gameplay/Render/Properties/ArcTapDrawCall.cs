using UnityEngine;

namespace ArcCreate.Gameplay.Render
{
    public struct ArcTapDrawCall
    {
        public bool IsSfx;
        public Texture Texture;
        public Matrix4x4 Matrix;
        public Color Color;
        public Vector4 Properties;
        public float Depth;
    }
}