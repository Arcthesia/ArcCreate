using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class Window : MonoBehaviour
    {
        private RectTransform rect;
        private RectTransform parent;

        [SerializeField] private Button toggleVisibilityButton;
        [SerializeField] private GameObject hideIndicator;
        [SerializeField] private GameObject showIndicator;
        [SerializeField] private GameObject content;
        [SerializeField] private WindowDragHandler dragHandler;
        [SerializeField] private float minDistanceFromBorderX = 10;
        [SerializeField] private float minDistanceFromBorderY = 50;
        private bool isVisible;

        private Vector2 mouseStartDragAt;
        private Vector2 positionStartDragAt;

        public bool IsVisible
        {
            get => isVisible;
            set
            {
                isVisible = value;
                AlignPivotDependingOnYPos();
                hideIndicator.SetActive(isVisible);
                showIndicator.SetActive(!isVisible);
                content.SetActive(isVisible);
                StartAligningSelfToScreen().Forget();
            }
        }

        private void AlignPivotDependingOnYPos()
        {
            float x = rect.anchoredPosition.x;
            float y = rect.anchoredPosition.y;
            float rectHeight = rect.rect.height;
            Vector2 pivot = rect.pivot;

            if (y + (rectHeight / 2) - (pivot.y * rectHeight) >= 0)
            {
                float moveY = (pivot.y - 1) * rect.sizeDelta.y;
                rect.pivot = new Vector2(pivot.x, 1);
                rect.anchoredPosition = new Vector2(x, y - moveY);
            }
            else
            {
                float moveY = pivot.y * rect.sizeDelta.y;
                rect.pivot = new Vector2(pivot.x, 0);
                rect.anchoredPosition = new Vector2(x, y - moveY);
            }
        }

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            parent = transform.parent.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            dragHandler.OnBeginDrag += OnBeginDrag;
            dragHandler.OnDrag += OnDrag;
            toggleVisibilityButton.onClick.AddListener(ToggleVisibility);
            IsVisible = true;
        }

        private void OnDestroy()
        {
            dragHandler.OnBeginDrag += OnBeginDrag;
            dragHandler.OnDrag += OnDrag;
            toggleVisibilityButton.onClick.AddListener(ToggleVisibility);
        }

        private void OnDrag(Vector2 pos)
        {
            Vector2 moveTo = positionStartDragAt + pos - mouseStartDragAt;
            AlignSelfToScreen(moveTo);
        }

        private void OnBeginDrag(Vector2 pos)
        {
            mouseStartDragAt = pos;
            positionStartDragAt = rect.anchoredPosition;
        }

        private void ToggleVisibility()
        {
            IsVisible = !IsVisible;
        }

        private async UniTask StartAligningSelfToScreen()
        {
            await UniTask.NextFrame();
            AlignSelfToScreen(rect.anchoredPosition);
        }

        private void AlignSelfToScreen(Vector2 position)
        {
            float parentWidth = parent.rect.width;
            float parentHeight = parent.rect.height;
            float rectWidth = rect.rect.width;
            float rectHeight = rect.rect.height;
            Vector2 pivot = rect.pivot;

            float x = Mathf.Clamp(
                position.x,
                -(parentWidth / 2) + minDistanceFromBorderX + (rectWidth * pivot.x),
                (parentWidth / 2) - minDistanceFromBorderX - (rectWidth * (1 - pivot.x)));

            float y = Mathf.Clamp(
                position.y,
                -(parentHeight / 2) + minDistanceFromBorderY + (rectHeight * pivot.y),
                (parentHeight / 2) - minDistanceFromBorderY - (rectHeight * (1 - pivot.y)));

            rect.anchoredPosition = new Vector2(x, y);

            // pivot = 0 => y + rectHeight / 2
            // pivot = 1 => y - rectHeight / 2
        }
    }
}