using System;
using System.Collections;
using UnityEngine;

namespace ArcCreate.Compose.Components
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class Row<T> : MonoBehaviour
    {
        private RectTransform rect;
        [SerializeField] private RectTransform scrollableFields;

        public RectTransform RectTransform => rect;

        public Table<T> Table { get; set; }

        public abstract void SetReference(T datum);

        public abstract void RemoveReference();

        public abstract void SetInteractable(bool interactable);

        public void SetHorizontalScroll(float x)
        {
            scrollableFields.anchoredPosition = new Vector2(
                x,
                scrollableFields.anchoredPosition.y);
        }

        protected void Awake()
        {
            rect = GetComponent<RectTransform>();
        }
    }
}