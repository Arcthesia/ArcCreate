using UnityEngine;

namespace ArcCreate.Compose.Grid
{
    public static class MeshBuilder
    {
        public static Mesh BuildQuadMeshVertical(Rect worldSpaceRect)
        {
            Vector3[] vertices = new Vector3[4];
            Vector2[] uv = new Vector2[4];
            int[] triangles = new int[6];

            float minX = worldSpaceRect.x;
            float minY = worldSpaceRect.y;
            float maxX = minX + worldSpaceRect.width;
            float maxY = minY + worldSpaceRect.height;

            vertices[0] = new Vector3(minX, minY, 0);
            uv[0] = new Vector2(0, 0);
            vertices[1] = new Vector3(maxX, minY, 0);
            uv[1] = new Vector2(1, 0);
            vertices[2] = new Vector3(maxX, maxY, 0);
            uv[2] = new Vector2(1, 1);
            vertices[3] = new Vector3(minX, maxY, 0);
            uv[3] = new Vector2(0, 1);

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;

            return new Mesh()
            {
                vertices = vertices,
                uv = uv,
                triangles = triangles,
            };
        }

        public static Mesh BuildQuadMeshLane(float fromX, float toX)
        {
            Vector3[] vertices = new Vector3[4];
            Vector2[] uv = new Vector2[4];
            int[] triangles = new int[6];

            float depth = Gameplay.Values.TrackLengthForward;

            vertices[0] = new Vector3(fromX, 0, -depth);
            uv[0] = new Vector2(0, 0);
            vertices[1] = new Vector3(toX, 0, -depth);
            uv[1] = new Vector2(1, 0);
            vertices[2] = new Vector3(toX, 0, 0);
            uv[2] = new Vector2(1, 1);
            vertices[3] = new Vector3(fromX, 0, 0);
            uv[3] = new Vector2(0, 1);

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;

            return new Mesh()
            {
                vertices = vertices,
                uv = uv,
                triangles = triangles,
            };
        }

        public static Mesh BuildGenericVerticalMesh(Vector3[] points)
        {
            Vector2[] uv = new Vector2[points.Length];
            int[] triangles = new int[(points.Length - 2) * 3];

            for (int i = 0; i < points.Length; i++)
            {
                uv[i] = default;
            }

            for (int tri = 0; tri < points.Length - 2; tri++)
            {
                triangles[(tri * 3) + 0] = 0;
                triangles[(tri * 3) + 1] = tri + 1;
                triangles[(tri * 3) + 2] = tri + 2;
            }

            return new Mesh()
            {
                vertices = points,
                uv = uv,
                triangles = triangles,
            };
        }
    }
}