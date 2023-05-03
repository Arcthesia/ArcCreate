using System.Collections.Generic;
using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;

namespace ArcCreate.Compose.Macros
{
    public class DropdownMacroField : BaseField
    {
        [SerializeField] private TMP_Dropdown dropdown;

        public override void SetupField(DialogField field, MacroRequest request)
        {
            base.SetupField(field, request);
            List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();

            int i = 0;
            foreach (DynValue obj in field.DropdownOptions)
            {
                dropdownOptions.Add(new TMP_Dropdown.OptionData(obj.String));
                i++;
            }

            dropdown.options = dropdownOptions;
            dropdown.value = (int)(field.DefaultValue?.Number ?? 0);
        }

        public override void UpdateResult()
        {
            Request.Result[Key] = DynValue.NewString(dropdown.options[dropdown.value].text);
        }

        public override bool IsFieldValid()
        {
            return true;
        }
    }
}