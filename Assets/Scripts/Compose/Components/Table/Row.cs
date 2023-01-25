using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Components
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class Row<T> : MonoBehaviour
    {
        private RectTransform rect;

        public RectTransform RectTransform
        {
            get
            {
                if (rect == null)
                {
                    rect = GetComponent<RectTransform>();
                }

                return rect;
            }
        }

        public T Reference { get; protected set; }

        public Table<T> Table { get; set; }

        public abstract bool Highlighted { get; set; }

        public abstract void SetReference(T datum);

        public abstract void RemoveReference();

        public abstract void SetInteractable(bool interactable);
    }
}