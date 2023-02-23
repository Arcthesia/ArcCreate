using UnityEngine;

namespace ArcCreate.Gameplay.Render
{
    public struct ShadowRenderProperties
    {
        public float From;
        public Vector4 Color;

        public static int Size()
        {
            return sizeof(float)
                 + (sizeof(float) * 4);
        }
    }
}