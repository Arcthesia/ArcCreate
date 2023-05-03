using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Macros
{
    public class DescriptionMacroField : BaseField
    {
        public override void SetupField(DialogField field, MacroRequest request)
        {
            base.SetupField(field, request);
            float height = Label.preferredHeight;
            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public override bool IsFieldValid()
        {
            return true;
        }

        public override void UpdateResult()
        {
        }

        public override void OnPointerEnter(PointerEventData data)
        {
        }

        public override void OnPointerExit(PointerEventData data)
        {
        }
    }
}