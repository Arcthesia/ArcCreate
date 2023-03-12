using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Components
{
    public class SliderRange : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Camera editorCamera;
        [SerializeField] private RectTransform container;
        [SerializeField] private RectTransform content;
        [SerializeField] private float dragHandleWidth;
        [SerializeField] private float minSize = 0;
        private DragMode dragMode = DragMode.None;
        private float startDragDistanceToLeft;
        private float startDragDistanceToRight;
        private float val1;
        private float val2;

        public event Action<float, float> OnValueChanged;

        public event Action<float, float> OnEndEdit;

        private enum DragMode
        {
            None,
            Start,
            Body,
            End,
        }

        public float From => Mathf.Min(val1, val2);

        public float To => Mathf.Max(val1, val2);

        public void SetValue(float from, float to)
        {
            SetValueWithoutNotify(from, to);
            OnValueChanged?.Invoke(From, To);
            OnEndEdit?.Invoke(From, To);
        }

        public void SetValueWithoutNotify(float from, float to)
        {
            val1 = val1 < val2 ? from : to;
            val2 = val1 < val2 ? to : from;
            UpdateVisual();
        }

        public void OnDrag(PointerEventData ev)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, ev.position, editorCamera, out Vector2 local);
            float pivot = Mathf.Clamp(local.x / container.rect.width, -0.5f, 0.5f) + 0.5f;

            switch (dragMode)
            {
                case DragMode.Start:
                    val1 = pivot;
                    break;
                case DragMode.End:
                    val2 = pivot;
                    break;
                case DragMode.Body:
                    val1 = pivot - startDragDistanceToLeft;
                    val2 = pivot + startDragDistanceToRight;
                    break;
            }

            UpdateVisual();
            OnValueChanged?.Invoke(From, To);
        }

        public void OnEndDrag(PointerEventData ev)
        {
            OnEndEdit?.Invoke(val1, val2);
            dragMode = DragMode.None;
        }

        public void OnPointerDown(PointerEventData ev)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container, ev.position, editorCamera, out Vector2 local);
            float pivot = Mathf.Clamp(local.x / container.rect.width, -0.5f, 0.5f) + 0.5f;

            if (pivot >= content.anchorMin.x
             && pivot <= content.anchorMin.x + (dragHandleWidth / container.rect.width))
            {
                dragMode = val1 < val2 ? DragMode.Start : DragMode.End;
            }
            else if (pivot <= content.anchorMax.x
                  && pivot >= content.anchorMax.x - (dragHandleWidth / container.rect.width))
            {
                dragMode = val1 < val2 ? DragMode.End : DragMode.Start;
            }
            else if (pivot >= content.anchorMin.x && pivot <= content.anchorMax.x)
            {
                dragMode = DragMode.Body;
                startDragDistanceToLeft = pivot - content.anchorMin.x;
                startDragDistanceToRight = content.anchorMax.x - pivot;
            }
        }

        private void UpdateVisual()
        {
            float from = From;
            float to = To;
            float viewSize = Mathf.Max(to - from, minSize / container.rect.width);
            content.anchorMin = new Vector2((from + to - viewSize) / 2, content.anchorMin.y);
            content.anchorMax = new Vector2((from + to + viewSize) / 2, content.anchorMax.y);
        }
    }
}