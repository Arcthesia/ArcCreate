using UnityEngine;

namespace ArcCreate.Utility.Extension
{
    public static class LineRendererExtension
    {
        /// <summary>
        /// Configure the line renderer to draw a line between two points.
        /// </summary>
        /// <param name="line">The line renderer.</param>
        /// <param name="from">The first point.</param>
        /// <param name="to">The second point.</param>
        public static void DrawLine(this LineRenderer line, Vector3 from, Vector3 to)
        {
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
        }
    }
}