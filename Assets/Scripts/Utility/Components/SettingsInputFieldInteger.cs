using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;

namespace ArcCreate
{
    [RequireComponent(typeof(TMP_InputField))]
    public class SettingsInputFieldInteger : MonoBehaviour
    {
        private TMP_InputField input;
        private IntSetting setting;

        private TMP_InputField Input
        {
            get
            {
                input = input == null ? GetComponent<TMP_InputField>() : input;
                return input;
            }
        }

        public void Setup(IntSetting setting)
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

        private void OnSettingChange(int value)
        {
            Input.SetTextWithoutNotify(value.ToString());
        }

        private void OnUIChange(string value)
        {
            if (Evaluator.TryInt(value, out int v))
            {
                setting.Value = v;
            }

            Input.SetTextWithoutNotify(setting.Value.ToString());
        }
    }
}