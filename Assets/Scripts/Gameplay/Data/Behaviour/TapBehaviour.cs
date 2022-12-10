using System.Collections.Generic;
using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class TapBehaviour : MonoBehaviour
    {
        private static Pool<LineRenderer> connectionLinePool;
        private static Quaternion baseLocalRotation;
        private static Vector3 baseLocalScale;
        private SpriteRenderer spriteRenderer;
        private readonly List<LineRenderer> connectionLines = new List<LineRenderer>(2);

        public Tap Tap { get; private set; }

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

        public void SetConnectionLines(List<ArcTap> arcTaps, Vector3 tapWorldPos)
        {
            for (int i = 0; i < connectionLines.Count; i++)
            {
                LineRenderer line = connectionLines[i];
                connectionLinePool.Return(line);
            }

            connectionLines.Clear();

            for (int i = 0; i < arcTaps.Count; i++)
            {
                ArcTap arcTap = arcTaps[i];
                Vector3 arcTapPos = new Vector3(arcTap.WorldX, arcTap.WorldY);
                Vector3 dist = arcTapPos - tapWorldPos;

                LineRenderer line = connectionLinePool.Get(transform);
                line.DrawLine(Vector3.zero, dist);
                connectionLines.Add(line);
            }
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseLocalRotation = transform.localRotation;
            baseLocalScale = transform.localScale;
        }
    }
}