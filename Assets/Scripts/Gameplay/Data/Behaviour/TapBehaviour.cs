using System.Collections.Generic;
using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class TapBehaviour : NoteBehaviour
    {
        private static Pool<LineRenderer> connectionLinePool;
        private static Quaternion baseLocalRotation;
        private static Vector3 baseLocalScale;
        private static MaterialPropertyBlock mpb;
        private static readonly int SelectedShaderId = Shader.PropertyToID("_Selected");

        private SpriteRenderer spriteRenderer;
        private readonly List<LineRenderer> connectionLines = new List<LineRenderer>(2);

        public Tap Tap { get; private set; }

        public override Note Note => Tap;

        public void SetData(Tap tap)
        {
            Tap = tap;
            connectionLinePool =
                connectionLinePool ??
                Pools.Get<LineRenderer>(Values.ConnectonLinePoolName);
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

        public void SetSelected(bool value)
        {
            spriteRenderer.GetPropertyBlock(mpb);
            mpb.SetInt(SelectedShaderId, value ? 1 : 0);
            spriteRenderer.SetPropertyBlock(mpb);
        }

        public void SetConnectionLines(HashSet<ArcTap> arcTaps, Vector3 tapWorldPos)
        {
            for (int i = 0; i < connectionLines.Count; i++)
            {
                LineRenderer line = connectionLines[i];
                connectionLinePool.Return(line);
            }

            connectionLines.Clear();

            foreach (ArcTap arcTap in arcTaps)
            {
                Vector3 arcTapPos = new Vector3(arcTap.WorldX, arcTap.WorldY);

                LineRenderer line = connectionLinePool.Get(transform, false);
                line.DrawLine(Vector3.zero, new Vector3(
                    (arcTapPos.x - tapWorldPos.x) / baseLocalScale.x,
                    0,
                    arcTapPos.y - tapWorldPos.y));

                connectionLines.Add(line);
            }
        }

        private void Awake()
        {
            mpb = new MaterialPropertyBlock();
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseLocalRotation = transform.localRotation;
            baseLocalScale = transform.localScale;
        }
    }
}