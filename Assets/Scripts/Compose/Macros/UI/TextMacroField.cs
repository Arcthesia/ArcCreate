﻿using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;

namespace ArcCreate.Compose.Macros
{
    public class TextMacroField : BaseField
    {
        [SerializeField] private TMP_InputField textField;
        [SerializeField] private TMP_Text invalidFeedback;
        [SerializeField] private float baseHeight;

        private FieldConstraint constraint;

        public override void SetupField(DialogField field, MacroRequest request)
        {
            base.SetupField(field, request);
            constraint = field.FieldConstraint;
            textField.text = field.DefaultValue?.CastToString() ?? "";

            switch (constraint.Type)
            {
                case FieldConstraint.InputType.Any:
                    textField.characterValidation = TMP_InputField.CharacterValidation.None;
                    break;
                case FieldConstraint.InputType.Float:
                    textField.characterValidation = TMP_InputField.CharacterValidation.Decimal;
                    break;
                case FieldConstraint.InputType.Integer:
                    textField.characterValidation = TMP_InputField.CharacterValidation.Integer;
                    break;
            }
        }

        public override void UpdateResult()
        {
            string value = textField.text;
            (bool valid, string message) = constraint.CheckValue(DynValue.NewString(value ?? ""));
            if (valid)
            {
                invalidFeedback.enabled = false;
                invalidFeedback.text = "";
                Request.Result[Key] = DynValue.NewString(value);
            }
            else
            {
                invalidFeedback.enabled = true;
                invalidFeedback.text = message;
            }

            float height = (valid ? 0 : invalidFeedback.preferredHeight) + baseHeight;
            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public override bool IsFieldValid()
        {
            return constraint.CheckValue(DynValue.NewString(textField.text)).valid;
        }
    }
}