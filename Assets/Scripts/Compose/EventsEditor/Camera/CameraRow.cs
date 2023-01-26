using System.Collections.Generic;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.History;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.EventsEditor
{
    public class CameraRow : Row<CameraEvent>, IPointerClickHandler
    {
        [SerializeField] private GameObject highlight;
        [SerializeField] private GameObject nonFields;
        [SerializeField] private TMP_InputField timingField;
        [SerializeField] private TMP_InputField durationField;
        [SerializeField] private TMP_InputField moveXField;
        [SerializeField] private TMP_InputField moveYField;
        [SerializeField] private TMP_InputField moveZField;
        [SerializeField] private TMP_InputField rotXField;
        [SerializeField] private TMP_InputField rotYField;
        [SerializeField] private TMP_InputField rotZField;
        [SerializeField] private TMP_Dropdown easingField;

        public override bool Highlighted
        {
            get => highlight.activeSelf;
            set => highlight.SetActive(value);
        }

        public override void RemoveReference()
        {
            Reference = null;
            timingField.SetTextWithoutNotify(string.Empty);
            durationField.SetTextWithoutNotify(string.Empty);
            moveXField.SetTextWithoutNotify(string.Empty);
            moveYField.SetTextWithoutNotify(string.Empty);
            moveZField.SetTextWithoutNotify(string.Empty);
            rotXField.SetTextWithoutNotify(string.Empty);
            rotYField.SetTextWithoutNotify(string.Empty);
            rotZField.SetTextWithoutNotify(string.Empty);
            easingField.SetValueWithoutNotify(0);
            highlight.SetActive(false);
        }

        public override void SetInteractable(bool interactable)
        {
            timingField.interactable = interactable;
            durationField.interactable = interactable;
            moveXField.interactable = interactable;
            moveYField.interactable = interactable;
            moveZField.interactable = interactable;
            rotXField.interactable = interactable;
            rotYField.interactable = interactable;
            rotZField.interactable = interactable;
            easingField.interactable = interactable;
            nonFields.SetActive(interactable);

            if (!interactable)
            {
                highlight.SetActive(false);
            }
        }

        public override void SetReference(CameraEvent datum)
        {
            Reference = datum;
            timingField.SetTextWithoutNotify(Reference.Timing.ToString());
            durationField.SetTextWithoutNotify(Reference.Duration.ToString());
            moveXField.SetTextWithoutNotify(Reference.Move.x.ToString());
            moveYField.SetTextWithoutNotify(Reference.Move.y.ToString());
            moveZField.SetTextWithoutNotify(Reference.Move.z.ToString());
            rotXField.SetTextWithoutNotify(Reference.Rotate.x.ToString());
            rotYField.SetTextWithoutNotify(Reference.Rotate.y.ToString());
            rotZField.SetTextWithoutNotify(Reference.Rotate.z.ToString());
            easingField.SetValueWithoutNotify((int)Reference.CameraType);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Table.Selected = Reference;
        }

        private void Awake()
        {
            timingField.onEndEdit.AddListener(OnField);
            durationField.onEndEdit.AddListener(OnField);
            moveXField.onEndEdit.AddListener(OnField);
            moveYField.onEndEdit.AddListener(OnField);
            moveZField.onEndEdit.AddListener(OnField);
            rotXField.onEndEdit.AddListener(OnField);
            rotYField.onEndEdit.AddListener(OnField);
            rotZField.onEndEdit.AddListener(OnField);
            easingField.onValueChanged.AddListener(OnDropdown);

            timingField.onSelect.AddListener(OnFieldSelect);
            durationField.onSelect.AddListener(OnFieldSelect);
            moveXField.onSelect.AddListener(OnFieldSelect);
            moveYField.onSelect.AddListener(OnFieldSelect);
            moveZField.onSelect.AddListener(OnFieldSelect);
            rotXField.onSelect.AddListener(OnFieldSelect);
            rotYField.onSelect.AddListener(OnFieldSelect);
            rotZField.onSelect.AddListener(OnFieldSelect);
        }

        private void OnDestroy()
        {
            timingField.onEndEdit.RemoveListener(OnField);
            durationField.onEndEdit.RemoveListener(OnField);
            moveXField.onEndEdit.RemoveListener(OnField);
            moveYField.onEndEdit.RemoveListener(OnField);
            moveZField.onEndEdit.RemoveListener(OnField);
            rotXField.onEndEdit.RemoveListener(OnField);
            rotYField.onEndEdit.RemoveListener(OnField);
            rotZField.onEndEdit.RemoveListener(OnField);
            easingField.onValueChanged.AddListener(OnDropdown);

            timingField.onSelect.RemoveListener(OnFieldSelect);
            durationField.onSelect.RemoveListener(OnFieldSelect);
            moveXField.onSelect.RemoveListener(OnFieldSelect);
            moveYField.onSelect.RemoveListener(OnFieldSelect);
            moveZField.onSelect.RemoveListener(OnFieldSelect);
            rotXField.onSelect.RemoveListener(OnFieldSelect);
            rotYField.onSelect.RemoveListener(OnFieldSelect);
            rotZField.onSelect.RemoveListener(OnFieldSelect);
        }

        private void OnFieldSelect(string arg)
        {
            Table.Selected = Reference;
        }

        private void OnField(string arg)
        {
            OnChange();
        }

        private void OnDropdown(int arg)
        {
            OnChange();
        }

        private void OnChange()
        {
            if (Evaluator.TryInt(timingField.text, out int timing)
             && Evaluator.TryInt(durationField.text, out int duration)
             && Evaluator.TryFloat(moveXField.text, out float mx)
             && Evaluator.TryFloat(moveYField.text, out float my)
             && Evaluator.TryFloat(moveZField.text, out float mz)
             && Evaluator.TryFloat(rotXField.text, out float rx)
             && Evaluator.TryFloat(rotYField.text, out float ry)
             && Evaluator.TryFloat(rotZField.text, out float rz))
            {
                var type = (Gameplay.Data.CameraType)easingField.value;

                if (timing != Reference.Timing || duration != Reference.Duration || type != Reference.CameraType
                 || mx != Reference.Move.x || my != Reference.Move.y || mz != Reference.Move.z
                 || rx != Reference.Rotate.x || ry != Reference.Rotate.y || rz != Reference.Rotate.z)
                {
                    CameraEvent newValue = new CameraEvent()
                    {
                        Timing = timing,
                        Duration = duration,
                        Move = new Vector3(mx, my, mz),
                        Rotate = new Vector3(rx, ry, rz),
                        CameraType = (Gameplay.Data.CameraType)easingField.value,
                        TimingGroup = Reference.TimingGroup,
                    };

                    Services.History.AddCommand(new EventCommand(
                        update: new List<(ArcEvent instance, ArcEvent newValue)> { (Reference, newValue) }));

                    ((CameraTable)Table).Rebuild();
                }
            }

            timingField.SetTextWithoutNotify(Reference.Timing.ToString());
            durationField.SetTextWithoutNotify(Reference.Duration.ToString());
            moveXField.SetTextWithoutNotify(Reference.Move.x.ToString());
            moveYField.SetTextWithoutNotify(Reference.Move.y.ToString());
            moveZField.SetTextWithoutNotify(Reference.Move.z.ToString());
            rotXField.SetTextWithoutNotify(Reference.Rotate.x.ToString());
            rotYField.SetTextWithoutNotify(Reference.Rotate.y.ToString());
            rotZField.SetTextWithoutNotify(Reference.Rotate.z.ToString());
            easingField.SetValueWithoutNotify((int)Reference.CameraType);
            ((CameraTable)Table).UpdateMarker();
        }
    }
}