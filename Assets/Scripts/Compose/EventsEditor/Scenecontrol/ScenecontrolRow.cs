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
        [SerializeField] private RectTransform fieldTransform;
        [SerializeField] private GameObject fieldPrefab;
        [SerializeField] private TMP_InputField timingField;
        [SerializeField] private float timingFieldRatio = 0.2f;
        [SerializeField] private float maxNumVisibleFields = 4;
        private readonly List<TMP_InputField> fields = new List<TMP_InputField>();

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

        public void SetupFields(int paramCount)
        {
            foreach (TMP_InputField field in fields)
            {
                field.onEndEdit.RemoveAllListeners();
                Destroy(field.gameObject);
            }

            fields.Clear();

            RectTransform rect = timingField.GetComponent<RectTransform>();
            float timingFieldWidth = paramCount == 0 ? 1 : timingFieldRatio;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(timingFieldWidth, 1);

            if (paramCount > 0)
            {
                float fieldWidth = Mathf.Max((1 - timingFieldRatio) / maxNumVisibleFields, (1 - timingFieldRatio) / paramCount);
                for (int i = 0; i < paramCount; i++)
                {
                    GameObject go = Instantiate(fieldPrefab, fieldTransform);
                    RectTransform r = go.GetComponent<RectTransform>();
                    r.anchorMin = new Vector2(fieldWidth * i, 0);
                    r.anchorMax = new Vector2(fieldWidth * (i + 1), 1);

                    TMP_InputField field = go.GetComponent<TMP_InputField>();
                    field.onEndEdit.AddListener(OnEndEdit);
                    fields.Add(field);
                }
            }

            SetInteractable(timingField.interactable);
        }

        public void SetFieldOffsetX(float x)
        {
            fieldTransform.anchoredPosition = new Vector2(x, fieldTransform.anchoredPosition.y);
        }

        public override void SetReference(ScenecontrolEvent datum)
        {
            Reference = datum;
            SetupFields((Table as ScenecontrolTable).ArgCount);
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
            try
            {
                int timing = Evaluator.Int(timingField.text);
                string[] args = fields.Select(field => field.text).ToArray();
                RegisterChange(timing, args);
            }
            catch (Exception)
            {
            }
        }

        private void OnDestroy()
        {
            foreach (TMP_InputField field in fields)
            {
                field.onEndEdit.RemoveAllListeners();
            }
        }
    }
}