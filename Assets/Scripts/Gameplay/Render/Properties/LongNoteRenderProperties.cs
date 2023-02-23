using UnityEngine;

namespace ArcCreate.Gameplay.Render
{
    public struct LongNoteRenderProperties
    {
        public float From;
        public Vector4 Color;
        public int Selected;

        public static int Size()
        {
            return sizeof(float)
                 + (sizeof(float) * 4)
                 + sizeof(int);
        }
    }
}