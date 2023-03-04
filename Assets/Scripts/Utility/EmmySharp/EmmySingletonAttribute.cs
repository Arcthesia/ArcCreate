/// <summary>
/// EmmySharp, created by Dylan Rafael (floofer++) for use by 0thElement.
/// The below classes help to generate EmmyLua workspace files from MoonSharp classes.
/// </summary>

using System;

namespace EmmySharp
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EmmySingletonAttribute : Attribute
    {
        public EmmySingletonAttribute()
        {
        }
    }
}