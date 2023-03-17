using UnityEngine;

namespace ArcCreate.Utility.Extension
{
    public static class VectorExtension
    {
        /// <summary>
        /// Multiply two vectors term by term.
        /// Note: This is neither dot product nor cross product.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The result.</returns>
        public static Vector3 Multiply(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        /// <summary>
        /// Multiply two vectors term by term.
        /// Note: This is neither dot product nor cross product.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The result.</returns>
        public static Vector2 Multiply(this Vector2 a, Vector2 b)
        {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        /// <summary>
        /// The dot product of two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The dot product.</returns>
        public static float Dot(this Vector3 a, Vector3 b)
        {
            return (a.x * b.x) + (a.y * b.y) + (a.z * b.z);
        }

        /// <summary>
        /// Whether the vector contains NaN in any of it's coordinate.
        /// </summary>
        /// <param name="vec">The vector to checl.</param>
        /// <returns>The bool value.</returns>
        public static bool IsNaN(this Vector3 vec)
        {
            return float.IsNaN(vec.x) || float.IsNaN(vec.y) || float.IsNaN(vec.z);
        }
    }
}