using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Components
{
    public class WindowDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public event Action<Vector2> OnBeginDrag;

        public event Action<Vector2> OnDrag;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDrag?.Invoke(eventData.position);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            OnDrag?.Invoke(eventData.position);
        }
    }
}