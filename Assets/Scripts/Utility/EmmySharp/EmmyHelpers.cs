/// <summary>
/// EmmySharp, created by Dylan Rafael (floofer++) for use by 0thElement.
/// The below classes help to generate EmmyLua workspace files from MoonSharp classes.
/// </summary>

using System;
using System.Linq;
using System.Reflection;

namespace EmmySharp
{
    public static class EmmyHelpers
    {
        public static string ToCamelCase(this string self)
        {
            if (self.Length == 0)
            {
                return self;
            }

            if (char.IsUpper(self[0]))
            {
                return char.ToLower(self[0]) + self.Substring(1);
            }

            return self;
        }

        public static T GetAttrOr<T>(this ICustomAttributeProvider attrs)
            where T : Attribute
        {
            if (attrs == null)
            {
                return null;
            }

            var customs = attrs.GetCustomAttributes(typeof(T), false);

            if (!customs.Any())
            {
                return null;
            }

            return (T)customs[0];
        }

        public static string EmmyDoc(this ICustomAttributeProvider p)
            => p.GetAttrOr<EmmyDocAttribute>()?.Documentation;

        public static string[] EmmyChoice(this ICustomAttributeProvider p)
            => p.GetAttrOr<EmmyChoiceAttribute>()?.Values;

        public static string EmmyAlias(this ICustomAttributeProvider p)
            => p.GetAttrOr<EmmyAliasAttribute>()?.Alias;
        public static string EmmyDeprecated(this ICustomAttributeProvider p)
            => p.GetAttrOr<EmmyDeprecatedAttribute>()?.DeprecationNotice;
    }
}