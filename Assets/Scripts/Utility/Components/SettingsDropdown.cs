using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ArcCreate
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class SettingsDropdown : MonoBehaviour
    {
        private TMP_Dropdown dropdown;
        private IntSetting setting;
        private Array enumValues;

        private TMP_Dropdown Dropdown
        {
            get
            {
                dropdown = dropdown == null ? GetComponent<TMP_Dropdown>() : dropdown;
                return dropdown;
            }
        }

        public void Setup(IntSetting setting, Type enumType, string i18nKey)
        {
            enumValues = Enum.GetValues(enumType);
            this.setting = setting;
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (object enumValue in enumValues)
            {
                options.Add(new TMP_Dropdown.OptionData(I18n.S($"{i18nKey}.{enumValue.ToString().ToLower()}")));
            }

            Dropdown.options = options;
            setting.OnValueChanged.AddListener(OnSettingChange);
            OnSettingChange(setting.Value);
        }

        private void Awake()
        {
            Dropdown.onValueChanged.AddListener(OnUIChange);
        }

        private void OnDestroy()
        {
            Dropdown.onValueChanged.RemoveListener(OnUIChange);
            setting?.OnValueChanged.RemoveListener(OnSettingChange);
        }

        private void OnSettingChange(int value)
        {
            for (int i = 0; i < enumValues.Length; i++)
            {
                object obj = enumValues.GetValue(i);
                if ((int)obj == value)
                {
                    Dropdown.SetValueWithoutNotify(i);
                    return;
                }
            }
        }

        private void OnUIChange(int value)
        {
            setting.Value = (int)enumValues.GetValue(value);
        }
    }
}