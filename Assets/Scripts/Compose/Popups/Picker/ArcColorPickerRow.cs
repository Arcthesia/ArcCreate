using ArcCreate.Compose.Components;
using ArcCreate.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Popups
{
    public class ArcColorPickerRow : Row<ColorSetting>, IPointerClickHandler
    {
        [SerializeField] private GameObject highlight;
        [SerializeField] private TMP_Text label;
        [SerializeField] private UIGradient gradient;
        private bool interactable;

        public override bool Highlighted
        {
            get => highlight.activeSelf;
            set => highlight.SetActive(value);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (interactable)
            {
                Table.Selected = Reference;
            }
        }

        public override void RemoveReference()
        {
            Reference = new ColorSetting { Id = int.MinValue };
            label.gameObject.SetActive(false);
            gradient.gameObject.SetActive(false);
            highlight.SetActive(false);
        }

        public override void SetInteractable(bool interactable)
        {
            this.interactable = interactable;
            label.gameObject.SetActive(interactable);
            gradient.gameObject.SetActive(interactable);
            highlight.SetActive(interactable);
        }

        public override void SetReference(ColorSetting datum)
        {
            Reference = datum;
            ChartSettings chart = Services.Project.CurrentChart;
            gradient.m_color1 = datum.High;
            gradient.m_color2 = datum.Low;

            switch (datum.Id)
            {
                case 0:
                    label.text = I18n.S("Compose.UI.Project.Label.Blue");
                    break;
                case 1:
                    label.text = I18n.S("Compose.UI.Project.Label.Red");
                    break;
                case 2:
                    label.text = I18n.S("Compose.UI.Project.Label.Green");
                    break;
                default:
                    label.text = I18n.S("Compose.UI.Project.Label.Custom", datum.Id);
                    break;
            }
        }
    }
}