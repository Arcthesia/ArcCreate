using UnityEngine;

namespace ArcCreate.Utility.Extension
{
    public static class LineRendererExtension
    {
        public static void DrawLine(this LineRenderer line, Vector3 from, Vector3 to)
        {
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
        }
    }
}