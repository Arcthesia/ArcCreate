using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class ArcTapBehaviour : MonoBehaviour
    {
        private static readonly int ColorShaderId = Shader.PropertyToID("_Color");
        private static MaterialPropertyBlock mpb;
        private static Quaternion baseLocalRotation;
        private static Vector3 baseLocalScale;

        [SerializeField] private SpriteRenderer shadowRenderer;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        private Transform shadowTransform;

        public ArcTap ArcTap { get; private set; }

        public void SetColor(Color color)
        {
            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(ColorShaderId, color);
            meshRenderer.SetPropertyBlock(mpb);

            shadowRenderer.color = color;
        }

        public void SetData(ArcTap arcTap)
        {
            ArcTap = arcTap;
        }

        public void SetSkin(Mesh mesh, Material mat)
        {
            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial = mat;
        }

        public void SetTransform(Vector3 pos, Quaternion rot, Vector3 scl)
        {
            transform.localPosition = pos;
            transform.localRotation = baseLocalRotation * rot;
            transform.localScale = baseLocalScale.Multiply(scl);

            Vector3 shadowPos = shadowTransform.localPosition;
            shadowPos.z = -transform.position.z;
            shadowTransform.localPosition = shadowPos;
        }

        public void Awake()
        {
            shadowTransform = shadowRenderer.transform;

            mpb = new MaterialPropertyBlock();
            meshRenderer.sortingLayerName = "Arc";
            meshRenderer.sortingOrder = 4;
            shadowRenderer.sortingLayerName = "Arc";
            shadowRenderer.sortingOrder = 3;
        }
    }
}