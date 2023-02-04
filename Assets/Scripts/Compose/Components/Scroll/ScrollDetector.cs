using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Components
{
    // I'm not stupid, Unity is.
    public class ScrollDetector : MonoBehaviour, IScrollHandler
    {
        private ScrollReceiver passToComponent;

        public void OnScroll(PointerEventData eventData)
        {
            passToComponent.ReceiveScroll(eventData);
        }

        private void Awake()
        {
            passToComponent = GetComponentInParent<ScrollReceiver>();
        }
    }
}