using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    public enum DialogFieldType
    {
        TextField,
        Dropdown,
        Checkbox,
        Description,
    }

    [MoonSharpUserData]
    [EmmyDoc("Description for constructing a dialog field.")]
    [EmmyGroup("Macros")]
    public class DialogField
    {
        public string Key { get; private set; }

        public string Label { get; private set; }

        public string Tooltip { get; private set; }

        public string Hint { get; private set; }

        public DynValue[] DropdownOptions { get; private set; }

        public DynValue DefaultValue { get; private set; }

        public FieldConstraint FieldConstraint { get; private set; }

        public DialogFieldType FieldType { get; private set; }

        [EmmyDoc("Create a new dialog field. The provided key is used for accessing this field's result value.")]
        public static DialogField Create(string key)
        {
            DialogField field = new DialogField
            {
                Key = key,
                Label = "New Field",
                Tooltip = "",
                Hint = "",
                FieldType = DialogFieldType.TextField,
                FieldConstraint = FieldConstraint.Create().Any(),
            };
            return field;
        }

        [EmmyDoc("Set the field's label.")]
        public DialogField SetLabel(string label)
        {
            Label = label;
            return this;
        }

        [EmmyDoc("Set the field's tooltip, which is shown when user hovers over the field.")]
        public DialogField SetTooltip(string tooltip)
        {
            Tooltip = tooltip;
            return this;
        }

        [EmmyDoc("Set the field's hint, which is shown when the field is empty.")]
        public DialogField SetHint(string hint)
        {
            Hint = hint;
            return this;
        }

        [EmmyDoc("Set the field's default value.")]
        public DialogField DefaultTo(DynValue value)
        {
            DefaultValue = value;
            return this;
        }

        [EmmyDoc("Change the field type to a text input field. Only accepts text input that fits the provided constraint.")]
        public DialogField TextField(FieldConstraint constraint)
        {
            FieldType = DialogFieldType.TextField;
            if (constraint == null)
            {
                FieldConstraint = FieldConstraint.Create().Any();
            }
            else
            {
                FieldConstraint = constraint;
            }

            return this;
        }

        [EmmyDoc("Change the field type to a dropdown field.")]
        public DialogField DropdownMenu(params DynValue[] options)
        {
            FieldType = DialogFieldType.Dropdown;
            DropdownOptions = options;
            return this;
        }

        [EmmyDoc("Change the field type to a checkbox field.")]
        public DialogField Checkbox()
        {
            FieldType = DialogFieldType.Checkbox;
            return this;
        }

        [EmmyDoc("Change the field type to a description field.")]
        public DialogField Description(string description = null)
        {
            if (description != null)
            {
                Label = description;
            }

            FieldType = DialogFieldType.Description;
            return this;
        }
    }
}