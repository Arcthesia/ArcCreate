using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.History;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.EventsEditor
{
    public class ScenecontrolRow : Row<ScenecontrolEvent>, IPointerClickHandler
    {
        [SerializeField] private GameObject highlight;
        [SerializeField] private GameObject highlightTiming;
        [SerializeField] private RectTransform fieldTransform;
        [SerializeField] private GameObject fieldPrefab;
        [SerializeField] private TMP_InputField timingField;
        [SerializeField] private RectTransform timingRect;
        [SerializeField] private float timingFieldRatio = 0.2f;
        private readonly List<TMP_InputField> fields = new List<TMP_InputField>();
        private float scrollableLength;

        public override bool Highlighted
        {
            get => highlight.activeSelf;
            set
            {
                highlight.SetActive(value);
                highlightTiming.SetActive(value);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Reference != null)
            {
                Table.Selected = Reference;
            }
        }

        public override void RemoveReference()
        {
            Reference = null;
            highlight.SetActive(false);
            timingField.gameObject.SetActive(false);
            foreach (var field in fields)
            {
                field.gameObject.SetActive(false);
            }
        }

        public override void SetInteractable(bool interactable)
        {
            timingField.interactable = interactable;
            foreach (TMP_InputField field in fields)
            {
                field.interactable = interactable;
            }

            if (!interactable)
            {
                highlight.SetActive(false);
            }
        }

        public void SetupFields(int paramCount, int maxNumVisibleFields)
        {
            foreach (TMP_InputField field in fields)
            {
                field.onEndEdit.RemoveAllListeners();
                field.onSelect.RemoveAllListeners();
                Destroy(field.gameObject);
            }

            fields.Clear();

            float timingFieldWidth = paramCount == 0 ? 1 : timingFieldRatio;
            timingRect.anchorMin = Vector2.zero;
            timingRect.anchorMax = new Vector2(timingFieldWidth, 1);
            timingRect.offsetMin = Vector2.zero;
            timingRect.offsetMax = Vector2.zero;

            if (paramCount > 0)
            {
                float fieldWidth = Mathf.Max((1 - timingFieldRatio) / maxNumVisibleFields, (1 - timingFieldRatio) / paramCount);
                scrollableLength = (fieldWidth * paramCount) - 1 + timingFieldWidth;

                for (int i = 0; i < paramCount; i++)
                {
                    GameObject go = Instantiate(fieldPrefab, fieldTransform);
                    RectTransform r = go.GetComponent<RectTransform>();
                    r.anchorMin = new Vector2(timingFieldRatio + (fieldWidth * i), 0);
                    r.anchorMax = new Vector2(timingFieldRatio + (fieldWidth * (i + 1)), 1);
                    r.offsetMin = Vector2.zero;
                    r.offsetMax = Vector2.zero;

                    TMP_InputField field = go.GetComponent<TMP_InputField>();
                    field.onEndEdit.AddListener(OnEndEdit);
                    field.onSelect.AddListener(OnFieldSelect);
                    fields.Add(field);
                }
            }

            SetInteractable(timingField.interactable);
        }

        public void SetFieldOffsetX(float x)
        {
            fieldTransform.anchorMin = new Vector2(-x * scrollableLength, 0);
            fieldTransform.anchorMax = new Vector2(1 - (x * scrollableLength), 1);
        }

        public override void SetReference(ScenecontrolEvent datum)
        {
            Reference = datum;
            SetupFields((Table as ScenecontrolTable).ArgCount, (Table as ScenecontrolTable).MaxNumVisibleFields);
            timingField.gameObject.SetActive(true);
            if (Reference == null)
            {
                foreach (var field in fields)
                {
                    field.text = "";
                }

                timingField.text = "";
            }
            else
            {
                for (int i = 0; i < fields.Count; i++)
                {
                    fields[i].text = (i >= Reference.Arguments.Count) ? "0" : Reference.Arguments[i].ToString();
                }

                timingField.text = Reference.Timing.ToString();
            }
        }

        private void RegisterChange(int timing, string[] args)
        {
            ScenecontrolEvent n = Reference.Clone() as ScenecontrolEvent;

            int oldTiming = n.Timing;

            // Check for change
            bool changed = false;
            if (timing != n.Timing)
            {
                changed = true;
            }

            if (args.Length != n.Arguments.Count)
            {
                changed = true;
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] != n.Arguments[i].ToString())
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (changed)
            {
                List<object> newArgs = new List<object>();
                foreach (string arg in args)
                {
                    object res;
                    if (Evaluator.TryFloat(arg, out float val))
                    {
                        res = val;
                    }
                    else
                    {
                        res = arg;
                    }

                    newArgs.Add(res);
                }

                n.Arguments = newArgs;
                n.Timing = timing;
                Services.History.AddCommand(new EventCommand(
                    name: I18n.S("Compose.Notify.History.EditScenecontrol"),
                    update: new List<(ArcEvent instance, ArcEvent newValue)> { (Reference, n) }));
            }

            for (int i = 0; i < args.Length; i++)
            {
                fields[i].text = n.Arguments[i].ToString();
            }

            timingField.text = timing.ToString();
        }

        private void OnEndEdit(string val)
        {
            if (!Evaluator.TryInt(timingField.text, out int timing))
            {
                return;
            }

            string[] args = fields.Select(field => field.text).ToArray();
            RegisterChange(timing, args);
        }

        private void OnFieldSelect(string arg)
        {
            Table.Selected = Reference;
        }

        private void Awake()
        {
            timingField.onEndEdit.AddListener(OnEndEdit);
            timingField.onSelect.AddListener(OnFieldSelect);
        }

        private void OnDestroy()
        {
            timingField.onEndEdit.RemoveListener(OnEndEdit);
            timingField.onSelect.RemoveListener(OnFieldSelect);
            foreach (TMP_InputField field in fields)
            {
                field.onEndEdit.RemoveAllListeners();
                field.onSelect.RemoveAllListeners();
            }
        }
    }
}