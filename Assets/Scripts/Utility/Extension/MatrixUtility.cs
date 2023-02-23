using UnityEngine;

namespace ArcCreate.Utility
{
    public static class MatrixUtility
    {
        public static Matrix4x4 Shear(Vector3 dir)
        {
            var matrix = new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(dir.x, dir.y, dir.z, 0),
                new Vector4(0, 0, 0, 1));
            return matrix;
        }
    }
}