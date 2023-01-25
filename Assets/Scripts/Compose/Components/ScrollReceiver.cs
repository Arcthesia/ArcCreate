using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Components
{
    // I'm not stupid, Unity is.
    public class ScrollReceiver : MonoBehaviour
    {
        public event Action<PointerEventData> OnScroll;

        public void ReceiveScroll(PointerEventData ev)
        {
            OnScroll?.Invoke(ev);
        }
    }
}