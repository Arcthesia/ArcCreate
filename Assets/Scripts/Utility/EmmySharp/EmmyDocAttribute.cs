/// <summary>
/// EmmySharp, created by Dylan Rafael (floofer++) for use by 0thElement.
/// The below classes help to generate EmmyLua workspace files from MoonSharp classes.
/// </summary>

using System;

namespace EmmySharp
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Class)]
    public class EmmyDocAttribute : Attribute
    {
        public EmmyDocAttribute(string doc)
        {
            Documentation = doc;
        }

        public string Documentation { get; }
    }
}