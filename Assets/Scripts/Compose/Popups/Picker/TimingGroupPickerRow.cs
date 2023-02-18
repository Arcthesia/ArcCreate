using ArcCreate.Compose.Components;
using ArcCreate.Gameplay.Chart;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Popups
{
    public class TimingGroupPickerRow : Row<TimingGroup>, IPointerClickHandler
    {
        [SerializeField] private GameObject highlight;
        [SerializeField] private TMP_Text nameField;
        [SerializeField] private TMP_Text fileText;

        public override bool Highlighted
        {
            get => highlight.activeSelf;
            set => highlight.SetActive(value);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Reference != null)
            {
                Table.Selected = Reference;
                (Table as TimingGroupPicker).SetValue(Reference);
                (Table as TimingGroupPicker).CloseWindow();
            }
        }

        public override void RemoveReference()
        {
            Reference = null;
            nameField.gameObject.SetActive(false);
            fileText.gameObject.SetActive(false);
            highlight.SetActive(false);
        }

        public override void SetInteractable(bool interactable)
        {
            if (!interactable)
            {
                highlight.SetActive(false);
            }
        }

        public override void SetReference(TimingGroup datum)
        {
            Reference = datum;
            nameField.gameObject.SetActive(true);
            fileText.gameObject.SetActive(true);

            if (Reference.GroupNumber == 0)
            {
                nameField.text = "Base group";
            }
            else
            {
                if (string.IsNullOrEmpty(Reference.GroupProperties.Name))
                {
                    nameField.text = $"Group {Reference.GroupNumber}";
                }
                else
                {
                    nameField.text = $"{Reference.GroupProperties.Name} ({Reference.GroupNumber})";
                }
            }

            fileText.text = Reference.GroupProperties.FileName;
        }
    }
}