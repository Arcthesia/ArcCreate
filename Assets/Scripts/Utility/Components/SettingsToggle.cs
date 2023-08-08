using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate
{
    [RequireComponent(typeof(Toggle))]
    public class SettingsToggle : MonoBehaviour
    {
        private Toggle toggle;
        private BoolSetting setting;

        private Toggle Toggle
        {
            get
            {
                toggle = toggle == null ? GetComponent<Toggle>() : toggle;
                return toggle;
            }
        }

        public void Setup(BoolSetting setting)
        {
            this.setting = setting;
            setting.OnValueChanged.AddListener(OnSettingChange);
            OnSettingChange(setting.Value);
        }

        private void Awake()
        {
            Toggle.onValueChanged.AddListener(OnUIChange);
        }

        private void OnDestroy()
        {
            Toggle.onValueChanged.RemoveListener(OnUIChange);
            setting?.OnValueChanged.RemoveListener(OnSettingChange);
        }

        private void OnSettingChange(bool value)
        {
            Toggle.SetIsOnWithoutNotify(value);
        }

        private void OnUIChange(bool value)
        {
            setting.Value = value;
        }
    }
}