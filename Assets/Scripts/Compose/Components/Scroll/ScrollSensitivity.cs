using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollSensitivity : MonoBehaviour
    {
        private ScrollRect scrollRect;
        [SerializeField] private bool isVertical;

        private void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
            var setting = isVertical ? Settings.ScrollSensitivityVertical : Settings.ScrollSensitivityHorizontal;
            scrollRect.scrollSensitivity = setting.Value;
            setting.OnValueChanged.AddListener(OnScrollSensitivitySettings);
        }

        private void OnDestroy()
        {
            var setting = isVertical ? Settings.ScrollSensitivityVertical : Settings.ScrollSensitivityHorizontal;
            setting.OnValueChanged.RemoveListener(OnScrollSensitivitySettings);
        }

        private void OnScrollSensitivitySettings(float sensitivity)
        {
            scrollRect.scrollSensitivity = sensitivity;
        }
    }
}