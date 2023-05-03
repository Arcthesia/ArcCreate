using System.Collections.Generic;
using ArcCreate.Compose.Components;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Macros
{
    public class MacroDialog : Dialog
    {
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private RectTransform fieldParent;
        [SerializeField] private GameObject textFieldPrefab;
        [SerializeField] private GameObject dropdownFieldPrefab;
        [SerializeField] private GameObject descriptionFieldPrefab;
        [SerializeField] private GameObject checkboxFieldPrefab;
        [SerializeField] private TMP_Text tooltip;
        [SerializeField] private TMP_Text title;
        [SerializeField] private float maxHeight;
        [SerializeField] private float fieldPaddingTotal;
        [SerializeField] private VerticalLayoutGroup fieldLayoutGroup;
        private readonly List<BaseField> currentFields = new List<BaseField>();
        private MacroRequest currentRequest;
        private RectTransform rect;

        public void SetTitle(string dialogTitle)
        {
            title.text = dialogTitle;
        }

        public override void Close()
        {
            DestroyFields();
            base.Close();
        }

        public void CreateFields(IEnumerable<DialogField> fields, MacroRequest request)
        {
            DestroyFields();

            foreach (var field in fields)
            {
                GameObject go = null;
                switch (field.FieldType)
                {
                    case DialogFieldType.TextField:
                        go = Instantiate(textFieldPrefab, fieldParent);
                        break;
                    case DialogFieldType.Dropdown:
                        go = Instantiate(dropdownFieldPrefab, fieldParent);
                        break;
                    case DialogFieldType.Description:
                        go = Instantiate(descriptionFieldPrefab, fieldParent);
                        break;
                    case DialogFieldType.Checkbox:
                        go = Instantiate(checkboxFieldPrefab, fieldParent);
                        break;
                }

                BaseField f = go.GetComponent<BaseField>();
                f.SetupField(field, request);
                f.Tooltip = tooltip;
                currentFields.Add(f);
            }

            RebuildLayout().Forget();
            currentRequest = request;
        }

        private void DestroyFields()
        {
            foreach (var field in currentFields)
            {
                Destroy(field.gameObject);
            }

            currentFields.Clear();
        }

        private void Awake()
        {
            confirmButton.onClick.AddListener(OnConfirm);
            cancelButton.onClick.AddListener(OnCancel);
        }

        private void OnDestroy()
        {
            confirmButton.onClick.RemoveListener(OnConfirm);
            cancelButton.onClick.RemoveListener(OnCancel);
        }

        private void OnConfirm()
        {
            bool valid = true;
            foreach (var field in currentFields)
            {
                field.UpdateResult();
                if (!field.IsFieldValid())
                {
                    valid = false;
                }
            }

            RebuildLayout().Forget();
            if (valid)
            {
                currentRequest.Complete = true;
                Close();
            }
        }

        private async UniTask RebuildLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(fieldParent);
            await UniTask.NextFrame();

            if (rect == null)
            {
                rect = GetComponent<RectTransform>();
            }

            float height = fieldLayoutGroup.padding.top;
            foreach (var f in currentFields)
            {
                height += f.GetComponent<RectTransform>().sizeDelta.y + fieldLayoutGroup.spacing;
            }

            height += fieldLayoutGroup.padding.bottom - fieldLayoutGroup.spacing;

            fieldParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Min(maxHeight, fieldParent.sizeDelta.y + fieldPaddingTotal));
        }

        private void OnCancel()
        {
            Close();
        }
    }
}