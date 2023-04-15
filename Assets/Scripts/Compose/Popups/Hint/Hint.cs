using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Popups
{
    public class Hint : MonoBehaviour
    {
        [SerializeField] private Color[] colors;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Vector2 padding;
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private Vector2 offset;
        [SerializeField] private float minDistanceFromBorder;
        [SerializeField] private float maxWidth;
        private RectTransform rect;
        private RectTransform owner;
        private Camera ownerCamera;

        public void SetContent(Vector2 screenPosition, Severity severity, string text, RectTransform owner, Camera camera = null)
        {
            gameObject.SetActive(true);
            this.owner = owner;
            background.color = colors[(int)severity];
            this.text.text = text;
            ownerCamera = camera;

            SetPositionTo(screenPosition);
        }

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        private void Update()
        {
            Vector2 mousePos = Input.mousePosition;
            if (!RectTransformUtility.RectangleContainsScreenPoint(owner, mousePos, ownerCamera))
            {
                gameObject.SetActive(false);
            }
            else
            {
                SetPositionTo(mousePos);
            }
        }

        private void SetPositionTo(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out Vector2 position);
            position += offset;

            float rectWidth = Mathf.Min(text.preferredWidth, maxWidth);
            float rectHeight = text.preferredHeight;
            rect.sizeDelta = new Vector2(rectWidth, rectHeight) + (padding * 2);

            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;
            Vector2 pivot = rect.pivot;

            float x = Mathf.Clamp(
                position.x,
                -(canvasWidth / 2) + minDistanceFromBorder + (rectWidth * pivot.x),
                (canvasWidth / 2) - minDistanceFromBorder - (rectWidth * (1 - pivot.x)));

            float y = Mathf.Clamp(
                position.y,
                -(canvasHeight / 2) + minDistanceFromBorder + (rectHeight * pivot.y),
                (canvasHeight / 2) - minDistanceFromBorder - (rectHeight * (1 - pivot.y)));

            rect.anchoredPosition = new Vector2(x, y);
        }
    }
}