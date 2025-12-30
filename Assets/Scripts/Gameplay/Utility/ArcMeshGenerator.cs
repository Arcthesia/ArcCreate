using System;
using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.Utility
{
    public static class ArcMeshGenerator
    {
        private static Mesh cachedTraceShadowMesh;
        private static Mesh cachedArcShadowMesh;
        private static Mesh cachedTraceMesh;
        private static Mesh cachedArcMesh;
        private static Mesh cachedTraceHeadMesh;
        private static Mesh cachedArcHeadMesh;

        public static Mesh GetSegmentMesh(bool isTrace)
        {
            if (isTrace)
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

        public static Mesh GetShadowMesh(bool isTrace)
        {
            if (isTrace)
            {
                if (cachedTraceShadowMesh == null)
                {
                    cachedTraceShadowMesh = GenerateShadowMesh(Values.TraceMeshOffset);
                }

                return cachedTraceShadowMesh;
            }
            else
            {
                if (cachedArcShadowMesh == null)
                {
                    cachedArcShadowMesh = GenerateShadowMesh(Values.ArcMeshOffset);
                }

                return cachedArcShadowMesh;
            }
        }

        public static Mesh GetHeadMesh(bool isTrace)
        {
            if (isTrace)
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
                    0, 1, 4,
                    0, 4, 5,
                },
            };
        }

        public static Mesh GenerateShadowMesh(float offset)
        {
            float offsetHalf = offset / 2;

            // 1---2
            // |   |
            // |   |
            // 0---3
            return new Mesh()
            {
                vertices = new Vector3[]
                {
                    // Body segment
                    new Vector3(offset, 0, 0),
                    new Vector3(offset, 0, 1),
                    new Vector3(-offset, 0, 1),
                    new Vector3(-offset, 0, 0),
                },
                uv = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                },
                triangles = new int[]
                {
                    0, 1, 3,
                    1, 2, 3,
                },
            };
        }

        public static Mesh GenerateHeadMesh(float offset)
        {
            float offsetHalf = offset / 2;

            // . 0
            // ./|\
            // 1 | 3
            //  \2/
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

        public static void GenerateColliderTriangles(
            Arc arc, List<Vector3> vertices, List<int> triangles, Vector3 pos, Vector3 scl)
        {
            vertices.Clear();
            triangles.Clear();

            float offset = arc.IsTrace ? Values.TraceMeshOffset : Values.ArcMeshOffset;
            float offsetHalf = offset / 2;

            int segmentCount = Mathf.CeilToInt((arc.EndTiming - arc.Timing) / arc.SegmentLength);
            segmentCount = Mathf.Max(segmentCount, 1);
            float segmentLength = arc.SegmentLength;
            var tg = arc.TimingGroupInstance;

            float baseX = ArcFormula.ArcXToWorld(arc.XStart);
            float baseY = ArcFormula.ArcYToWorld(arc.YStart);
            float baseZ = ArcFormula.FloorPositionToZ(arc.FloorPosition, arc.TimingGroup);

            // help
            if (arc.IsFirstArcOfGroup)
            {
                // . 4
                //  /|\
                // 5 | 6
                // | 1 |
                // |/|\|
                // 2 | 3
                //  \0/
                bool arcIsBackward = tg.GetFloorPosition(Mathf.Min(arc.Timing + (int)segmentLength, arc.EndTiming)) < arc.FloorPosition;
                vertices.Add(new Vector3(0, -offsetHalf, arcIsBackward ? -offsetHalf : offsetHalf)); // 0
                vertices.Add(new Vector3(0, offsetHalf, 0)); // 1
                vertices.Add(new Vector3(offset, -offsetHalf, 0)); // 2
                vertices.Add(new Vector3(-offset, -offsetHalf, 0)); // 3

                triangles.Add(0); // 0
                triangles.Add(2); // 1
                triangles.Add(1); // 2
                triangles.Add(0); // 3
                triangles.Add(1); // 4
                triangles.Add(3); // 5

                for (int i = 0; i < segmentCount; i++)
                {
                    int timing = Mathf.RoundToInt(arc.Timing + (segmentLength * (i + 1)));
                    timing = Mathf.Min(timing, arc.EndTiming);
                    float x = timing == arc.EndTiming ? ArcFormula.ArcXToWorld(arc.XEnd) : arc.WorldXAt(timing);
                    float y = timing == arc.EndTiming ? ArcFormula.ArcYToWorld(arc.YEnd) : arc.WorldYAt(timing);
                    float z = ArcFormula.FloorPositionToZ(tg.GetFloorPosition(timing), arc.TimingGroup);

                    float dx = x - baseX;
                    float dy = y - baseY;
                    float dz = z - baseZ;

                    vertices.Add(new Vector3(dx, dy + offsetHalf, dz)); // 3i + 4
                    vertices.Add(new Vector3(dx + offset, dy - offsetHalf, dz)); // 3i + 5
                    vertices.Add(new Vector3(dx - offset, dy - offsetHalf, dz)); // 3i + 6

                    triangles.Add((i * 3) + 1); // 12i + 6
                    triangles.Add((i * 3) + 2); // 12i + 7
                    triangles.Add((i * 3) + 5); // 12i + 8

                    triangles.Add((i * 3) + 1); // 12i + 9
                    triangles.Add((i * 3) + 5); // 12i + 10
                    triangles.Add((i * 3) + 4); // 12i + 11

                    triangles.Add((i * 3) + 1); // 12i + 12
                    triangles.Add((i * 3) + 4); // 12i + 13
                    triangles.Add((i * 3) + 3); // 12i + 14

                    triangles.Add((i * 3) + 3); // 12i + 15
                    triangles.Add((i * 3) + 4); // 12i + 16
                    triangles.Add((i * 3) + 6); // 12i + 17
                }
            }
            else
            {
                // . 3
                //  / \
                // 4   5
                // | 0 |
                // |/ \|
                // 1   2
                vertices.Add(new Vector3(0, offsetHalf, 0)); // 0
                vertices.Add(new Vector3(offset, -offsetHalf, 0)); // 1
                vertices.Add(new Vector3(-offset, -offsetHalf, 0)); // 2
                for (int i = 0; i < segmentCount; i++)
                {
                    int timing = Mathf.RoundToInt(arc.Timing + (segmentLength * (i + 1)));
                    timing = Mathf.Min(timing, arc.EndTiming);
                    float x = timing == arc.EndTiming ? ArcFormula.ArcXToWorld(arc.XEnd) : arc.WorldXAt(timing);
                    float y = timing == arc.EndTiming ? ArcFormula.ArcYToWorld(arc.YEnd) : arc.WorldYAt(timing);
                    float z = ArcFormula.FloorPositionToZ(tg.GetFloorPosition(timing), arc.TimingGroup);

                    float dx = x - baseX;
                    float dy = y - baseY;
                    float dz = z - baseZ;

                    vertices.Add(new Vector3(dx, dy + offsetHalf, dz)); // 3i + 3
                    vertices.Add(new Vector3(dx + offset, dy - offsetHalf, dz)); // 3i + 4
                    vertices.Add(new Vector3(dx - offset, dy - offsetHalf, dz)); // 3i + 5

                    triangles.Add((i * 3) + 0); // 12i + 0
                    triangles.Add((i * 3) + 1); // 12i + 1
                    triangles.Add((i * 3) + 4); // 12i + 2

                    triangles.Add((i * 3) + 0); // 12i + 3
                    triangles.Add((i * 3) + 4); // 12i + 4
                    triangles.Add((i * 3) + 3); // 12i + 5

                    triangles.Add((i * 3) + 0); // 12i + 6
                    triangles.Add((i * 3) + 3); // 12i + 7
                    triangles.Add((i * 3) + 2); // 12i + 8

                    triangles.Add((i * 3) + 2); // 12i + 9
                    triangles.Add((i * 3) + 3); // 12i + 10
                    triangles.Add((i * 3) + 5); // 12i + 11
                }
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 v = vertices[i];
                v.Multiply(scl);
                v += pos;
                vertices[i] = v;
            }
        }
    }
}