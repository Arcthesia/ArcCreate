using UnityEngine;

namespace ArcCreate.Gameplay.Render
{
    public struct ArcRenderProperties
    {
        public float From;
        public Vector4 Color;
        public float RedValue;
        public int Selected;

        public static int Size()
        {
            return sizeof(float)
                 + (sizeof(float) * 4)
                 + sizeof(float)
                 + sizeof(int);
        }
    }
}