using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class TapBehaviour : MonoBehaviour
    {
        private static Quaternion baseLocalRotation;
        private static Vector3 baseLocalScale;
        private SpriteRenderer spriteRenderer;

        public Tap Tap { get; private set; }

        public void SetData(Tap tap)
        {
            Tap = tap;
        }

        public void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        public void SetTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            transform.localPosition = position;
            transform.localRotation = baseLocalRotation * rotation;
            transform.localScale = baseLocalScale.Multiply(scale);
        }

        public void SetColor(Color color)
        {
            spriteRenderer.color = color;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseLocalRotation = transform.localRotation;
            baseLocalScale = transform.localScale;
        }
    }
}