using ArcCreate.Compose.Components;
using Google.MaterialDesign.Icons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Macros
{
    public class LastRunMacroRow : Row<MacroDefinition>
    {
        [SerializeField] private MaterialIcon icon;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Button button;
        [SerializeField] private RectTransform textRect;
        [SerializeField] private float offsetMinWithIcon;
        [SerializeField] private float offsetMinWithoutIcon;
        private string macroId;

        public override bool Highlighted { get; set; }

        public override void RemoveReference()
        {
            macroId = null;
            text.text = string.Empty;
            icon.gameObject.SetActive(false);
        }

        public override void SetInteractable(bool interactable)
        {
            button.interactable = interactable;
        }

        public override void SetReference(MacroDefinition datum)
        {
            macroId = datum.Id;
            text.text = datum.Name;
            if (datum.Icon != null)
            {
                icon.iconUnicode = datum.Icon;
                icon.gameObject.SetActive(true);
                textRect.offsetMin = new Vector2(offsetMinWithIcon, textRect.offsetMin.y);
            }
            else
            {
                icon.gameObject.SetActive(false);
                textRect.offsetMin = new Vector2(offsetMinWithoutIcon, textRect.offsetMin.y);
            }
        }

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            Services.Macros.RunMacro(macroId);
        }
    }
}