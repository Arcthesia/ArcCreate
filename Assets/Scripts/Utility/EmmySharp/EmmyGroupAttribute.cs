/// <summary>
/// EmmySharp, created by Dylan Rafael (floofer++) for use by 0thElement.
/// The below classes help to generate EmmyLua workspace files from MoonSharp classes.
/// </summary>

using System;

namespace EmmySharp
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EmmyGroupAttribute : Attribute
    {
        public EmmyGroupAttribute(string groupName)
        {
            GroupName = groupName;
        }

        public string GroupName { get; }
    }
}