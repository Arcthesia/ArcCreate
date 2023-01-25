using System.Collections.Generic;
using ArcCreate.ChartFormat;
using ArcCreate.Compose.Components;
using ArcCreate.Gameplay.Chart;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
    public class GroupRow : Row<TimingGroup>, IPointerClickHandler
    {
        [SerializeField] private GameObject highlight;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_InputField propertiesField;
        [SerializeField] private Toggle visibilityButton;

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
                Values.EditingTimingGroup.Value = Reference.GroupNumber;
            }
        }

        public override void RemoveReference()
        {
            Reference = null;
            nameText.text = string.Empty;
            propertiesField.gameObject.SetActive(false);
            visibilityButton.gameObject.SetActive(false);
            highlight.SetActive(false);
        }

        public override void SetInteractable(bool interactable)
        {
            propertiesField.interactable = interactable;
            visibilityButton.interactable = interactable;

            if (Reference != null && Reference.GroupNumber == 0)
            {
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
            nameText.text = datum.GroupNumber.ToString();
            propertiesField.text = datum.GroupProperties.ToRaw().ToString();
            propertiesField.gameObject.SetActive(true);
            visibilityButton.gameObject.SetActive(true);
            visibilityButton.isOn = Reference.IsVisible;

            highlight.SetActive(Reference.GroupNumber == Values.EditingTimingGroup.Value);
            if (Reference.GroupNumber == 0)
            {
                propertiesField.interactable = false;
            }
        }

        private void Awake()
        {
            propertiesField.onEndEdit.AddListener(OnProperties);
            visibilityButton.onValueChanged.AddListener(OnVisiblity);
        }

        private void OnDestroy()
        {
            propertiesField.onEndEdit.RemoveListener(OnProperties);
            visibilityButton.onValueChanged.AddListener(OnVisiblity);
        }

        private void OnProperties(string value)
        {
            try
            {
                // TODO: Hook to undo/redo management
                RawTimingGroup group = new RawTimingGroup(value);
                Reference.SetGroupProperties(new Gameplay.Data.GroupProperties(group));
            }
            catch (ChartFormatException e)
            {
                throw new ComposeException(I18n.S("Compose.Exception.InvalidGroupProperties", new Dictionary<string, object>
                {
                    { "Message", e.Message },
                }));
            }
            finally
            {
                propertiesField.text = Reference.GroupProperties.ToRaw().ToString();
            }
        }

        private void OnVisiblity(bool vis)
        {
            Reference.IsVisible = vis;
        }
    }
}