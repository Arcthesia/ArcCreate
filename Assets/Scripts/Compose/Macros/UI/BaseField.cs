using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Macros
{
    public abstract class BaseField : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Text label;
        [HideInInspector] private string key;
        [HideInInspector] private string tooltipText;

        public TMP_Text Tooltip { get; set; }

        public RectTransform TooltipParent { get; set; }

        public virtual float PreferredHeight { get => GetComponent<RectTransform>().sizeDelta.y; }

        protected MacroRequest Request { get; set; }

        protected string Key => key;

        protected TMP_Text Label => label;

        public virtual void SetupField(DialogField field, MacroRequest request)
        {
            Request = request;
            key = field.Key;
            label.text = field.Label;
            tooltipText = field.Tooltip;

            if (request.Result.ContainsKey(key))
            {
                throw new System.Exception($"Duplicate dialog field key {key}");
            }

            request.Result.Add(key, MoonSharp.Interpreter.DynValue.Nil);
        }

        public virtual void OnPointerEnter(PointerEventData data)
        {
            Tooltip.text = tooltipText;
            Tooltip.enabled = true;
            TooltipParent.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(TooltipParent);
        }

        public virtual void OnPointerExit(PointerEventData data)
        {
            Tooltip.text = "";
            Tooltip.enabled = false;
            TooltipParent.gameObject.SetActive(false);
        }

        public abstract bool IsFieldValid();

        public abstract void UpdateResult();
    }
}