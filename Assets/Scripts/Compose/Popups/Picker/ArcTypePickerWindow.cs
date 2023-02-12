using System;
using ArcCreate.Compose.Components;
using ArcCreate.Gameplay.Data;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Popups
{
    public class ArcTypePickerWindow : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject window;
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private float minDistanceFromBorder;
        [SerializeField] private OptionsPanel optionsPanel;
        private RectTransform rect;
        private ArcLineType type;

        public Action<ArcLineType> OnTypeChanged { get; set; }

        public object Owner { get; private set; }

        public ArcLineType Type
        {
            get => type;
            set
            {
                type = value;
                optionsPanel.SetValueWithoutNotify(type.ToLineTypeString());
                OnTypeChanged?.Invoke(type);
            }
        }

        public void OpenAt(Vector2 screenPosition, ArcLineType? setType, object caller)
        {
            window.SetActive(true);
            Owner = caller;

            if (setType != null)
            {
                type = setType.Value;
                optionsPanel.SetValueWithoutNotify(type.ToLineTypeString());
            }
            else
            {
                optionsPanel.SetValueWithoutNotify(null);
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out Vector2 position);

            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;
            float rectWidth = rect.rect.width;
            float rectHeight = rect.rect.height;
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

        public void SetTypeWithoutNotify(ArcLineType value)
        {
            type = value;
            optionsPanel.SetValueWithoutNotify(value.ToLineTypeString());
        }

        private void CloseWindow()
        {
            window.SetActive(false);
        }

        private void Awake()
        {
            optionsPanel.OnSelect += OnOptionsPanel;
            rect = window.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            closeButton.onClick.AddListener(CloseWindow);
        }

        private void OnDestroy()
        {
            optionsPanel.OnSelect -= OnOptionsPanel;
            closeButton.onClick.RemoveListener(CloseWindow);
        }

        private void OnOptionsPanel(string typeString)
        {
            ArcLineType newType = typeString.ToArcLineType();
            Type = newType;
            CloseWindow();
        }
    }
}