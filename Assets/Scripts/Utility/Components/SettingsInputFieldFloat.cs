using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;

namespace ArcCreate
{
    [RequireComponent(typeof(TMP_InputField))]
    public class SettingsInputFieldFloat : MonoBehaviour
    {
        private TMP_InputField input;
        private FloatSetting setting;
        private float rounding;

        private TMP_InputField Input
        {
            get
            {
                input = input == null ? GetComponent<TMP_InputField>() : input;
                return input;
            }
        }

        public void Setup(FloatSetting setting, int rounding)
        {
            this.setting = setting;
            this.rounding = Mathf.Pow(10, rounding);
            setting.OnValueChanged.AddListener(OnSettingChange);
            OnSettingChange(setting.Value);
        }

        private void Awake()
        {
            Input.onEndEdit.AddListener(OnUIChange);
        }

        private void OnDestroy()
        {
            Input.onEndEdit.RemoveListener(OnUIChange);
            setting?.OnValueChanged.RemoveListener(OnSettingChange);
        }

        private void OnSettingChange(float value)
        {
            Input.SetTextWithoutNotify(value.ToString());
        }

        private void OnUIChange(string value)
        {
            if (Evaluator.TryFloat(value, out float v))
            {
                v = Mathf.Round(v * rounding) / rounding;
                setting.Value = v;
            }

            Input.SetTextWithoutNotify(setting.Value.ToString());
        }
    }
}