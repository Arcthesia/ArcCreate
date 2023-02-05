using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class HoldBehaviour : NoteBehaviour
    {
        private static readonly int FromShaderId = Shader.PropertyToID("_From");
        private static readonly int ShearShaderId = Shader.PropertyToID("_Shear");
        private static readonly int SelectedShaderId = Shader.PropertyToID("_Selected");
        private static MaterialPropertyBlock mpb;
        private static Quaternion baseLocalRotation;
        private static Vector3 baseLocalScale;
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

        public override Note Note => Hold;

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

        public void SetSelected(bool value)
        {
            spriteRenderer.GetPropertyBlock(mpb);
            mpb.SetInt(SelectedShaderId, value ? 1 : 0);
            spriteRenderer.SetPropertyBlock(mpb);
        }

        public void SetFallDirection(Vector3 dir)
        {
            dir = dir.normalized;
            spriteRenderer.GetPropertyBlock(mpb);
            mpb.SetVector(ShearShaderId, new Vector4(
                dir.x,
                dir.y,
                1,
                0));
            spriteRenderer.SetPropertyBlock(mpb);
        }

        public void SetFrom(float from)
        {
            spriteRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat(FromShaderId, from);
            spriteRenderer.SetPropertyBlock(mpb);
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            baseLocalRotation = transform.localRotation;
            baseLocalScale = transform.localScale;

            mpb = new MaterialPropertyBlock();
        }
    }
}