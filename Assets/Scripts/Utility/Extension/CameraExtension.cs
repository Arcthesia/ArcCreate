using UnityEngine;

namespace ArcCreate.Utility.Extension
{
    public static class CameraExtension
    {
        public static Ray ScreenPointToRay(this Camera camera)
        {
            return camera.ScreenPointToRay(Input.mousePosition);
        }
    }
}