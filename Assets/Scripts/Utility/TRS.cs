using System;
using UnityEngine;

namespace ArcCreate.Utility
{
    /// <summary>
    /// A simple data structure which represents a TRS transformation,
    /// implementing two operations, (+) and (*), to simplify their
    /// combination.
    /// The main reasoning for this type's existence is to dimplify access
    /// to the components of a TRS matrix transformation.
    /// </summary>
    public struct TRS
    {
        public TRS(Vector3 position, Quaternion rot, Vector3 scale)
        {
            Translation = position;
            Rotation = rot;
            Scale = scale;
        }

        public TRS(Matrix4x4 matrix)
        {
            if (!matrix.ValidTRS())
            {
                throw new InvalidOperationException("Cannot create a transform from an invalid TRS matrix!");
            }

            Translation = matrix.GetColumn(3);
            Rotation = matrix.rotation;
            Scale = matrix.lossyScale;

            // Debug.Log("Scale = " + Scale);
        }

        #pragma warning disable
        public static TRS identity
        #pragma warning restore
            => new TRS(Vector3.zero, Quaternion.identity, Vector3.one);

        public Vector3 Translation { get; set; }

        public Quaternion Rotation { get; set; }

        public Vector3 Scale { get; set; }

        public Matrix4x4 Matrix => Matrix4x4.TRS(Translation, Rotation, Scale);

        public static implicit operator Matrix4x4(TRS a)
            => a.Matrix;

        public static TRS operator +(TRS a, TRS b)
            => new TRS(
                a.Translation + b.Translation,
                a.Rotation * b.Rotation,
                new Vector3(a.Scale.x * b.Scale.x, a.Scale.y * b.Scale.y, a.Scale.z * b.Scale.z));

        public static TRS operator *(TRS a, TRS b)
            => new TRS(a.Matrix * b.Matrix);

        public static TRS TranslateOnly(Vector3 translate)
            => new TRS(translate, Quaternion.identity, Vector3.one);

        public static TRS ScaleOnly(Vector3 scale)
            => new TRS(Vector3.zero, Quaternion.identity, scale);
    }
}