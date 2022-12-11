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
                    cachedTraceMesh = GenerateMesh(Values.ArcMeshOffsetNormal);
                }

                return cachedTraceMesh;
            }
            else
            {
                if (cachedArcMesh == null)
                {
                    cachedArcMesh = GenerateMesh(Values.ArcMeshOffsetTrace);
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
                    cachedTraceHeadMesh = GenerateHeadMesh(Values.ArcMeshOffsetNormal);
                }

                return cachedTraceHeadMesh;
            }
            else
            {
                if (cachedArcHeadMesh == null)
                {
                    cachedArcHeadMesh = GenerateHeadMesh(Values.ArcMeshOffsetTrace);
                }

                return cachedArcHeadMesh;
            }
        }

        private static Mesh GenerateMesh(float offset)
        {
            float offsetHalf = offset / 2;
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

        private static Mesh GenerateHeadMesh(float offset)
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
    }
}