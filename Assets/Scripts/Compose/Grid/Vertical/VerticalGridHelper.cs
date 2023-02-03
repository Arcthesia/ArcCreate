using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Compose.Grid
{
    public static class VerticalGridHelper
    {
        public static Vector2 SnapPoint(
            List<Line> lines,
            List<Vector2> cachedIntersections,
            Vector2 point,
            float tolerance)
        {
            tolerance *= tolerance;
            Vector2 closest = point;
            float minDist = float.MaxValue;
            bool intersectionFound = false;

            for (int i = 0; i < cachedIntersections.Count; i++)
            {
                Vector2 intersection = cachedIntersections[i];

                float dist = (intersection - point).sqrMagnitude;
                if (dist > tolerance)
                {
                    continue;
                }

                if (dist < minDist)
                {
                    closest = intersection;
                    minDist = dist;
                    intersectionFound = true;
                }
            }

            if (intersectionFound)
            {
                return closest;
            }

            closest = point;
            minDist = float.MaxValue;
            for (int i = 0; i < lines.Count; i++)
            {
                Line line = lines[i];

                Vector2 p = ClosestPointToLine(line, point);
                float dist = (p - point).sqrMagnitude;
                if (dist > tolerance)
                {
                    continue;
                }

                if (dist < minDist)
                {
                    closest = p;
                    minDist = dist;
                }
            }

            return closest;
        }

        public static List<Vector2> PrecalculateIntersections(List<Line> lines)
        {
            List<Vector2> result = new List<Vector2>();

            for (int i = 0; i < lines.Count; i++)
            {
                Line line1 = lines[i];
                for (int j = 0; j < lines.Count; j++)
                {
                    Line line2 = lines[j];

                    if (LinesIntersect(out Vector2 point, line1, line2))
                    {
                        result.Add(point);
                    }
                }
            }

            return result;
        }

        public static Vector2 ClosestPointToLine(Line line, Vector2 point)
        {
            // Read: https://math.stackexchange.com/questions/2193720/find-a-point-on-a-line-segment-which-is-the-closest-to-other-point-not-on-the-li
            // All variables name are taken from there. I have no idea what else to call them anyway.
            Vector2 v = line.End - line.Start;
            Vector2 u = line.Start - point;

            float uv = (u.x * v.x) + (u.y * v.y);
            float vv = (v.x * v.x) + (v.y * v.y);

            float t = -uv / vv;
            if (t < 0)
            {
                return line.Start;
            }
            else if (t > 1)
            {
                return line.End;
            }
            else
            {
                return ((1 - t) * line.Start) + (t * line.End);
            }
        }

        public static bool LinesIntersect(out Vector2 point, Line line1, Line line2)
        {
            float ay_cy, ax_cx, px, py;
            float dx_cx = line2.End.x - line2.Start.x,
                dy_cy = line2.End.y - line2.Start.y,
                bx_ax = line1.End.x - line1.Start.x,
                by_ay = line1.End.y - line1.Start.y;

            float de = (bx_ax * dy_cy) - (by_ay * dx_cx);

            if (Mathf.Abs(de) < Mathf.Epsilon)
            {
                point = default;
                return false;
            }

            ax_cx = line1.Start.x - line2.Start.x;
            ay_cy = line1.Start.y - line2.Start.y;
            float r = ((ay_cy * dx_cx) - (ax_cx * dy_cy)) / de;
            float s = ((ay_cy * bx_ax) - (ax_cx * by_ay)) / de;
            px = line1.Start.x + (r * bx_ax);
            py = line1.Start.y + (r * by_ay);

            point = new Vector2(px, py);
            return true;
        }
    }
}