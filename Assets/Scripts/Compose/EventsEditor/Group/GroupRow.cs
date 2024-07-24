using System.Collections.Generic;
using ArcCreate.ChartFormat;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.History;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
    public class GroupRow : Row<TimingGroup>, IPointerClickHandler
    {
        [SerializeField] private GameObject highlight;
        [SerializeField] private GameObject clickBlocker;
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private TMP_Text fileText;
        [SerializeField] private TMP_InputField propertiesField;
        [SerializeField] private Toggle visibilityButton;

        private string previousNameDisplay;

        public override bool Highlighted
        {
            get => highlight.activeSelf;
            set
            {
                highlight.SetActive(value);
                clickBlocker.SetActive(!value);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Reference != null)
            {
                Table.Selected = Reference;
                Values.EditingTimingGroup.Value = Reference.GroupNumber;
            }
        }

        public override void RemoveReference()
        {
            Reference = null;
            nameField.gameObject.SetActive(false);
            fileText.gameObject.SetActive(false);
            propertiesField.gameObject.SetActive(false);
            visibilityButton.gameObject.SetActive(false);
            highlight.SetActive(false);
        }

        public override void SetInteractable(bool interactable)
        {
            nameField.interactable = interactable;
            propertiesField.interactable = interactable;
            visibilityButton.interactable = interactable;

            if (Reference != null && (Reference.GroupNumber == 0 || !Reference.GroupProperties.Editable))
            {
                nameField.interactable = false;
                propertiesField.interactable = false;
            }

            if (!interactable)
            {
                highlight.SetActive(false);
            }
        }

        public override void SetReference(TimingGroup datum)
        {
            Reference = datum;
            propertiesField.text = Reference.GroupProperties.ToRaw().ToStringWithoutName();
            nameField.gameObject.SetActive(true);
            fileText.gameObject.SetActive(true);
            propertiesField.gameObject.SetActive(true);
            visibilityButton.gameObject.SetActive(true);
            visibilityButton.isOn = Reference.IsVisible;

            highlight.SetActive(Reference.GroupNumber == Values.EditingTimingGroup.Value);

            if (Reference.GroupNumber == 0)
            {
                nameField.text = "Base group";
                propertiesField.gameObject.SetActive(false);
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

            previousNameDisplay = nameField.text;
            fileText.text = Reference.GroupProperties.FileName;
        }

        private void Awake()
        {
            nameField.onEndEdit.AddListener(OnName);
            propertiesField.onEndEdit.AddListener(OnProperties);
            visibilityButton.onValueChanged.AddListener(OnVisiblity);

            nameField.onSelect.AddListener(OnNameSelect);
        }

        private void OnDestroy()
        {
            nameField.onEndEdit.RemoveListener(OnName);
            propertiesField.onEndEdit.RemoveListener(OnProperties);
            visibilityButton.onValueChanged.RemoveListener(OnVisiblity);

            nameField.onSelect.RemoveListener(OnNameSelect);
        }

        private void OnNameSelect(string arg)
        {
            nameField.text = Reference.GroupProperties.Name ?? string.Empty;
        }

        private void OnName(string value)
        {
            if (value == previousNameDisplay)
            {
                return;
            }

            Values.ProjectModified = true;

            value = value.Trim();
            value = value.Replace("=", string.Empty);
            value = value.Replace(",", string.Empty);
            value = value.Replace("\"", string.Empty);

            // Duplicate group
            GroupProperties newProps = new GroupProperties(Reference.GroupProperties.ToRaw());
            if (string.IsNullOrEmpty(value))
            {
                newProps.Name = null;
                nameField.text = $"Group {Reference.GroupNumber}";
            }
            else
            {
                newProps.Name = value;
                nameField.text = $"{Reference.GroupProperties.Name} ({Reference.GroupNumber})";
            }

            previousNameDisplay = nameField.text;
            ICommand cmd = new EditTimingGroupProperitesCommand(I18n.S(
                "Compose.Notify.GroupTable.EditGroup", new Dictionary<string, object>
                {
                        { "Number", Reference.GroupNumber },
                }), Reference, newProps);
            Services.History.AddCommand(cmd);

            (Table as GroupTable).UpdateEditingGroupField();
        }

        private void OnProperties(string value)
        {
            RawTimingGroup group = RawTimingGroup.Parse(value).UnwrapOrElse(e =>
            {
                propertiesField.text = Reference.GroupProperties.ToRaw().ToStringWithoutName();
                throw new ComposeException(I18n.S("Compose.Exception.InvalidGroupProperties", new Dictionary<string, object>
                {
                    { "Message", e.Message },
                }));
            });

            group.File = Reference.GroupProperties.FileName;
            group.Name = Reference.GroupProperties.Name;
            ICommand cmd = new EditTimingGroupProperitesCommand(I18n.S(
                "Compose.Notify.GroupTable.EditGroup", new Dictionary<string, object>
                {
                        { "Number", Reference.GroupNumber },
                }), Reference, new GroupProperties(group));
            
            Services.History.AddCommand(cmd);
            Values.ProjectModified = true;
            Values.OnEditAction?.Invoke();
            propertiesField.text = Reference.GroupProperties.ToRaw().ToStringWithoutName();
        }

        private void OnVisiblity(bool vis)
        {
            Reference.IsVisible = vis;
        }
    }
}