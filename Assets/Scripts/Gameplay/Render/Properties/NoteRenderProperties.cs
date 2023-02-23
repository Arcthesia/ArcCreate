using UnityEngine;

namespace ArcCreate.Gameplay.Render
{
    public struct NoteRenderProperties
    {
        public Vector4 Color;
        public int Selected;

        public static int Size()
        {
            return (sizeof(float) * 4)
                 + sizeof(int);
        }
    }
}