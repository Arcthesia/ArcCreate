/// <summary>
/// EmmySharp, created by Dylan Rafael (floofer++) for use by 0thElement.
/// The below classes help to generate EmmyLua workspace files from MoonSharp classes.
/// </summary>

using System;

namespace EmmySharp
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Property | AttributeTargets.Field)]
    public class EmmyChoiceAttribute : Attribute
    {
        public EmmyChoiceAttribute(params string[] values)
        {
            Values = values;
        }

        public string[] Values { get; }
    }
}