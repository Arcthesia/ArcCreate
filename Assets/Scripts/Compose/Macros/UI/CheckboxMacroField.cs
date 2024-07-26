using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Macros
{
    public class CheckboxMacroField : BaseField
    {
        [SerializeField] private Toggle checkbox;

        public override void SetupField(DialogField field, MacroRequest request)
        {
            base.SetupField(field, request);
            checkbox.isOn = field.DefaultValue == null ? false :
                bool.TryParse(field.DefaultValue.String.ToLower(), out bool b) ? b :
                field.DefaultValue.CastToBool();
        }

        public override bool IsFieldValid()
        {
            return true;
        }

        public override void UpdateResult()
        {
            Request.Result[Key] = DynValue.NewBoolean(checkbox.isOn);
        }
    }
}