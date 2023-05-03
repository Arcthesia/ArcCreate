using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class ChartFilePathInputField : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Toggle toggleCustom;
        [SerializeField] private TMP_Dropdown presets;

        private void Awake()
        {
            presets.onValueChanged.AddListener(OnPresetSelect);
            toggleCustom.onValueChanged.AddListener(OnToggleCustom);
            presets.options = new System.Collections.Generic.List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData(I18n.S("Compose.UI.Project.Options.Past")),
                new TMP_Dropdown.OptionData(I18n.S("Compose.UI.Project.Options.Present")),
                new TMP_Dropdown.OptionData(I18n.S("Compose.UI.Project.Options.Future")),
                new TMP_Dropdown.OptionData(I18n.S("Compose.UI.Project.Options.Beyond")),
            };

            presets.SetValueWithoutNotify(2);
            inputField.SetTextWithoutNotify("2");
        }

        private void OnDestroy()
        {
            presets.onValueChanged.RemoveListener(OnPresetSelect);
            toggleCustom.onValueChanged.RemoveListener(OnToggleCustom);
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
            inputField.text = val.ToString();
        }
    }
}