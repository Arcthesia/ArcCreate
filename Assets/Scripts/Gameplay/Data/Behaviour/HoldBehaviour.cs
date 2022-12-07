using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class HoldBehaviour : MonoBehaviour
    {
        private static Quaternion baseLocalRotation;
        private static Vector3 baseLocalScale;
        private static int fromShaderId;
        private static int shearShaderId;
        private static MaterialPropertyBlock mpb;
        private SpriteRenderer spriteRenderer;
        private Sprite normalSprite;
        private Sprite highlightSprite;
        private bool highlight = false;

        public Hold Hold { get; private set; }

        public bool Highlight
        {
            get => highlight;
            set
            {
                highlight = value;
                spriteRenderer.sprite = highlight ? highlightSprite : normalSprite;
            }
        }

        public void SetData(Hold hold)
        {
            Hold = hold;
            Highlight = hold.Highlight;
        }

        public void SetSprite(Sprite normal, Sprite highlight)
        {
            normalSprite = normal;
            highlightSprite = highlight;
            spriteRenderer.sprite = this.Highlight ? highlightSprite : normalSprite;
        }

        public void SetTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Vector3 scl = baseLocalScale.Multiply(scale);
            scl.y *= Values.HoldLengthScalar;

            transform.localPosition = position;
            transform.localRotation = baseLocalRotation * rotation;
            transform.localScale = scl;
        }

        public void SetColor(Color color)
        {
            spriteRenderer.color = color;
        }

        public void SetFallDirection(Vector3 dir)
        {
            Vector3 displacement = dir * transform.lossyScale.z;
            spriteRenderer.GetPropertyBlock(mpb);
            mpb.SetVector(shearShaderId, new Vector4(
                displacement.x / displacement.z,
                displacement.y / displacement.z,
                0,
                0));
            spriteRenderer.SetPropertyBlock(mpb);
        }

        public void SetFrom(float from)
        {
            spriteRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat(fromShaderId, from);
            spriteRenderer.SetPropertyBlock(mpb);
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            baseLocalRotation = transform.localRotation;
            baseLocalScale = transform.localScale;

            fromShaderId = Shader.PropertyToID("_From");
            shearShaderId = Shader.PropertyToID("_Shear");

            mpb = mpb ?? new MaterialPropertyBlock();
        }
    }
}