using System;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay.Utility
{
    public static class ArcMeshGenerator
    {
        private static Mesh cachedTraceMesh;
        private static Mesh cachedArcMesh;
        private static Mesh cachedTraceHeadMesh;
        private static Mesh cachedArcHeadMesh;

        public static Mesh GetSegmentMesh(Arc arc)
        {
            if (arc.IsTrace)
            {
                if (cachedTraceMesh == null)
                {
                    cachedTraceMesh = GenerateMesh(Values.TraceMeshOffset);
                }

                return cachedTraceMesh;
            }
            else
            {
                if (cachedArcMesh == null)
                {
                    cachedArcMesh = GenerateMesh(Values.ArcMeshOffset);
                }

                return cachedArcMesh;
            }
        }

        public static Mesh GetHeadMesh(Arc arc)
        {
            if (arc.IsTrace)
            {
                if (cachedTraceHeadMesh == null)
                {
                    cachedTraceHeadMesh = GenerateHeadMesh(Values.TraceMeshOffset);
                }

                return cachedTraceHeadMesh;
            }
            else
            {
                if (cachedArcHeadMesh == null)
                {
                    cachedArcHeadMesh = GenerateHeadMesh(Values.ArcMeshOffset);
                }

                return cachedArcHeadMesh;
            }
        }

        public static Mesh GenerateMesh(float offset)
        {
            float offsetHalf = offset / 2;

            // . 1
            //  / \
            // 2   4
            // | 0 |
            // |/ \|
            // 3   5
            return new Mesh()
            {
                vertices = new Vector3[]
                {
                    // Body segment
                    new Vector3(0, offsetHalf, 0),
                    new Vector3(0, offsetHalf, 1),
                    new Vector3(offset, -offsetHalf, 1),
                    new Vector3(offset, -offsetHalf, 0),
                    new Vector3(-offset, -offsetHalf, 1),
                    new Vector3(-offset, -offsetHalf, 0),
                },
                uv = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                },
                triangles = new int[]
                {
                    0, 3, 2,
                    0, 2, 1,
                    0, 5, 4,
                    0, 4, 1,
                },
                bounds = new Bounds(Vector3.zero, Vector3.one * float.MaxValue),
            };
        }

        public static Mesh GenerateHeadMesh(float offset)
        {
            float offsetHalf = offset / 2;
            return new Mesh()
            {
                vertices = new Vector3[]
                {
                    new Vector3(0, offsetHalf, 0),
                    new Vector3(offset, -offsetHalf, 0),
                    new Vector3(0, -offsetHalf, offsetHalf),
                    new Vector3(-offset, -offsetHalf, 0),
                },
                uv = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(1, 1),
                },
                triangles = new int[]
                {
                    0, 2, 1,
                    0, 3, 2,
                },
            };
        }

        public static Mesh GenerateColliderMesh(Arc arc)
        {
            float offset = arc.IsTrace ? Values.TraceMeshOffset : Values.ArcMeshOffset;
            float offsetHalf = offset / 2;

            int segmentCount = Mathf.CeilToInt((arc.EndTiming - arc.Timing) / arc.SegmentLength);
            segmentCount = Mathf.Max(segmentCount, 1);
            float segmentLength = arc.SegmentLength;
            var tg = arc.TimingGroupInstance;

            float baseX = ArcFormula.ArcXToWorld(arc.XStart);
            float baseY = ArcFormula.ArcYToWorld(arc.YStart);
            float baseZ = ArcFormula.FloorPositionToZ(arc.FloorPosition);

            Vector3[] vertices = new Vector3[3 + (segmentCount * 3)];
            Vector2[] uv = new Vector2[3 + (segmentCount * 3)];
            int[] triangles = new int[12 * segmentCount];

            // . 3
            //  / \
            // 4   5
            // | 0 |
            // |/ \|
            // 1   2
            vertices[0] = new Vector3(0, offsetHalf, 0);
            vertices[1] = new Vector3(offset, -offsetHalf, 0);
            vertices[2] = new Vector3(-offset, -offsetHalf, 0);
            for (int i = 0; i < segmentCount; i++)
            {
                int timing = Mathf.RoundToInt(arc.Timing + (segmentLength * (i + 1)));
                timing = Mathf.Min(timing, arc.EndTiming);
                float x = timing == arc.EndTiming ? ArcFormula.ArcXToWorld(arc.XEnd) : arc.WorldXAt(timing);
                float y = timing == arc.EndTiming ? ArcFormula.ArcYToWorld(arc.YEnd) : arc.WorldYAt(timing);
                float z = ArcFormula.FloorPositionToZ(tg.GetFloorPosition(timing));

                float dx = x - baseX;
                float dy = y - baseY;
                float dz = z - baseZ;

                vertices[(i * 3) + 3] = new Vector3(dx, dy + offsetHalf, dz);
                vertices[(i * 3) + 4] = new Vector3(dx + offset, dy - offsetHalf, dz);
                vertices[(i * 3) + 5] = new Vector3(dx - offset, dy - offsetHalf, dz);

                triangles[(i * 12) + 0] = (i * 3) + 0;
                triangles[(i * 12) + 1] = (i * 3) + 1;
                triangles[(i * 12) + 2] = (i * 3) + 4;

                triangles[(i * 12) + 3] = (i * 3) + 0;
                triangles[(i * 12) + 4] = (i * 3) + 4;
                triangles[(i * 12) + 5] = (i * 3) + 3;

                triangles[(i * 12) + 6] = (i * 3) + 0;
                triangles[(i * 12) + 7] = (i * 3) + 3;
                triangles[(i * 12) + 8] = (i * 3) + 2;

                triangles[(i * 12) + 9] = (i * 3) + 2;
                triangles[(i * 12) + 10] = (i * 3) + 3;
                triangles[(i * 12) + 11] = (i * 3) + 5;
            }

            return new Mesh
            {
                vertices = vertices,
                uv = uv,
                triangles = triangles,
            };
        }
    }
}