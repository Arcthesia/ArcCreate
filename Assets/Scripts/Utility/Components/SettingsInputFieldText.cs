using TMPro;
using UnityEngine;

namespace ArcCreate
{
    [RequireComponent(typeof(TMP_InputField))]
    public class SettingsInputFieldText : MonoBehaviour
    {
        private TMP_InputField input;
        private StringSetting setting;

        private TMP_InputField Input
        {
            get
            {
                input = input == null ? GetComponent<TMP_InputField>() : input;
                return input;
            }
        }

        public void Setup(StringSetting setting)
        {
            this.setting = setting;
            setting.OnValueChanged.AddListener(OnSettingChange);
            OnSettingChange(setting.Value);
        }

        private void Awake()
        {
            Input.onValueChanged.AddListener(OnUIChange);
        }

        private void OnDestroy()
        {
            Input.onValueChanged.RemoveListener(OnUIChange);
            setting?.OnValueChanged.RemoveListener(OnSettingChange);
        }

        private void OnSettingChange(string value)
        {
            Input.SetTextWithoutNotify(value);
        }

        private void OnUIChange(string value)
        {
            setting.Value = value;
        }
    }
}