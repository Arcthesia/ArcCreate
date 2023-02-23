using UnityEngine;

namespace ArcCreate.Gameplay.Render
{
    public struct SpriteRenderProperties
    {
        public Vector4 Color;

        public static int Size()
        {
            return sizeof(float) * 4;
        }
    }
}