using TMPro;
using UnityEngine;

namespace ArcCreate.Compose.Components
{
    [RequireComponent(typeof(TMP_InputField))]
    public class ScrollSensitivityInputField : MonoBehaviour
    {
        private TMP_InputField inputField;

        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();
            inputField.scrollSensitivity = Settings.ScrollSensitivityVertical.Value / 10;
            Settings.ScrollSensitivityVertical.OnValueChanged.AddListener(OnScrollSensitivitySettings);
        }

        private void OnDestroy()
        {
            Settings.ScrollSensitivityVertical.OnValueChanged.RemoveListener(OnScrollSensitivitySettings);
        }

        private void OnScrollSensitivitySettings(float sensitivity)
        {
            inputField.scrollSensitivity = sensitivity / 10;
        }
    }
}