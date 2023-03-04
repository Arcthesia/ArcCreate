/// <summary>
/// EmmySharp, created by Dylan Rafael (floofer++) for use by 0thElement.
/// The below classes help to generate EmmyLua workspace files from MoonSharp classes.
/// </summary>

using System;

namespace EmmySharp
{
    public class EmmySharpValue
    {
        public EmmySharpValue(string doc, string name, Type type, string[] options = null, bool isStatic = true)
        {
            Doc = doc;
            Name = name;
            Type = type;
            Options = options;
            IsStatic = isStatic;
        }

        public EmmySharpValue(string name, Type type, string[] options = null, bool isStatic = true)
            : this(null, name, type, options, isStatic)
        {
        }

        public string Doc { get; }

        public string Name { get; }

        public string[] Options { get; }

        public Type Type { get; }

        public bool IsStatic { get; }
    }
}