using UnityEngine;

namespace ArcCreate.Utility.Extension
{
    public static class VectorExtension
    {
        public static Vector3 Multiply(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static float Dot(this Vector3 a, Vector3 b)
        {
            return (a.x * b.x) + (a.y * b.y) + (a.z * b.z);
        }
    }
}