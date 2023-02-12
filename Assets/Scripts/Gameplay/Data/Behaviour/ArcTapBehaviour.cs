using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class ArcTapBehaviour : NoteBehaviour
    {
        private static readonly int ColorShaderId = Shader.PropertyToID("_Color");
        private static readonly int SelectedShaderId = Shader.PropertyToID("_Selected");
        private static MaterialPropertyBlock mpb;
        private static Quaternion baseLocalRotation;
        private static Vector3 baseLocalScale;
        private static Color baseShadowColor;

        [SerializeField] private SpriteRenderer shadowRenderer;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        private Transform shadowTransform;

        public ArcTap ArcTap { get; private set; }

        public override Note Note => ArcTap;

        public void SetColor(Color color)
        {
            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(ColorShaderId, color);
            meshRenderer.SetPropertyBlock(mpb);

            shadowRenderer.color = baseShadowColor * color;
        }

        public void SetSelected(bool value)
        {
            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetInt(SelectedShaderId, value ? 1 : 0);
            meshRenderer.SetPropertyBlock(mpb);
        }

        public void SetData(ArcTap arcTap)
        {
            ArcTap = arcTap;
        }

        public void SetSkin(Mesh mesh, Material mat, Sprite shadow)
        {
            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial = mat;
            shadowRenderer.sprite = shadow;
        }

        public void SetTransform(Vector3 pos, Quaternion rot, Vector3 scl)
        {
            transform.localPosition = pos;
            transform.localRotation = baseLocalRotation * rot;
            transform.localScale = baseLocalScale.Multiply(scl);

            Vector3 shadowPos = shadowTransform.localPosition;
            shadowPos.y = -transform.position.y;
            shadowTransform.localPosition = shadowPos;
        }

        public void Awake()
        {
            shadowTransform = shadowRenderer.transform;
            baseLocalRotation = transform.localRotation;
            baseLocalScale = transform.localScale;

            mpb = new MaterialPropertyBlock();
            meshRenderer.sortingLayerName = "Arc";
            meshRenderer.sortingOrder = 4;
            shadowRenderer.sortingLayerName = "Arc";
            shadowRenderer.sortingOrder = 3;
            baseShadowColor = shadowRenderer.color;
        }
    }
}