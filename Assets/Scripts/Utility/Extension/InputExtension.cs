using UnityEngine;

namespace ArcCreate.Utility.Extension
{
    public static class InputExtension
    {
        public static Vector2 ScaledMousePosition
        {
            get
            {
                return ScaledMousePositionWithSize(new Vector2(1920, 1080));
            }
        }

        public static Vector2 ScaledMousePositionWithSize(Vector2 size)
        {
            float ratio = size.x / Screen.width;
            Vector2 mousePosition = Input.mousePosition;
            return mousePosition * ratio;
        }
    }
}