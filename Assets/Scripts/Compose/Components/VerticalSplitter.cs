using UnityEngine;

namespace ArcCreate.Compose.Components
{
    // Because VerticalLayoutGroup breaks literally everything
    [RequireComponent(typeof(RectTransform))]
    public class VerticalSplitter : MonoBehaviour
    {
        private RectTransform rect;
        [SerializeField] private RectTransform top;
        [SerializeField] private RectTransform bottom;
        [SerializeField] private float topRatio;
        [SerializeField] private float bottomRatio;

        private void Update()
        {
            top.sizeDelta = new Vector2(
                top.sizeDelta.x,
                rect.rect.height * topRatio);
            bottom.sizeDelta = new Vector2(
                bottom.sizeDelta.x,
                rect.rect.height * bottomRatio);

            bottom.anchoredPosition = new Vector2(
                bottom.anchoredPosition.x,
                -rect.rect.height * topRatio);
        }

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }
    }
}