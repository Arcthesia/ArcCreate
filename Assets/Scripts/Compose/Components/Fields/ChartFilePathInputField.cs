using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class ChartFilePathInputField : MonoBehaviour
    {
        private static readonly List<(string, TMP_Dropdown.OptionData)> Options = new List<(string, TMP_Dropdown.OptionData)>
        {
            ("0", new TMP_Dropdown.OptionData(I18n.S("Compose.UI.Project.Options.Past"))),
            ("1", new TMP_Dropdown.OptionData(I18n.S("Compose.UI.Project.Options.Present"))),
            ("2", new TMP_Dropdown.OptionData(I18n.S("Compose.UI.Project.Options.Future"))),
            ("4", new TMP_Dropdown.OptionData(I18n.S("Compose.UI.Project.Options.Eternal"))),
            ("3", new TMP_Dropdown.OptionData(I18n.S("Compose.UI.Project.Options.Beyond"))),
        };

        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Toggle toggleCustom;
        [SerializeField] private TMP_Dropdown presets;

        private void Awake()
        {
            presets.onValueChanged.AddListener(OnPresetSelect);
            toggleCustom.onValueChanged.AddListener(OnToggleCustom);
            ReloadOptions();
            I18n.OnLocaleChanged += ReloadOptions;

            presets.SetValueWithoutNotify(2);
            inputField.SetTextWithoutNotify("2");
        }

        private void ReloadOptions()
        {
            presets.options = Options.Select(e => e.Item2).ToList();
        }

        private void OnDestroy()
        {
            presets.onValueChanged.RemoveListener(OnPresetSelect);
            toggleCustom.onValueChanged.RemoveListener(OnToggleCustom);
            I18n.OnLocaleChanged -= ReloadOptions;
        }

        private void OnToggleCustom(bool val)
        {
            inputField.gameObject.SetActive(val);
            presets.gameObject.SetActive(!val);
            if (!val)
            {
                OnPresetSelect(presets.value);
            }
        }

        private void OnPresetSelect(int val)
        {
            inputField.text = Options[val].Item1;
        }
    }
}