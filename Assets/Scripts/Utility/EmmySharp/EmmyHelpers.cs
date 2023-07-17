/// <summary>
/// EmmySharp, created by Dylan Rafael (floofer++) for use by 0thElement.
/// The below classes help to generate EmmyLua workspace files from MoonSharp classes.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;

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

        public static T GetAttrOrNull<T>(this ICustomAttributeProvider attrs)
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
            => p.GetAttrOrNull<EmmyDocAttribute>()?.Documentation;

        public static EmmyType GetEmmyType(this ICustomAttributeProvider p, Type baseType, IReadOnlyDictionary<string, EmmyType> aliases)
        {
            EmmyType GetTypeNoMod()
            {
                var typeAttr = p.GetAttrOrNull<EmmyTypeAttribute>();

                if (typeAttr is null)
                {
                    var choicesAttr = p.GetAttrOrNull<EmmyChoiceAttribute>();

                    if (choicesAttr is null)
                    {
                        return EmmyType.From(baseType);
                    }
                    else
                    {
                        return EmmyType.Option(choicesAttr.Values);
                    }
                }
                else
                {
                    if (typeAttr.Raw is null)
                    {
                        return EmmyType.From(typeAttr.Type);
                    }

                    return EmmyType.Raw(typeAttr.Raw);
                }
            }

            var nullable = p.GetAttrOrNull<EmmyNullableAttribute>() != null;
            var ret = GetTypeNoMod();

            if (nullable)
            {
                ret = EmmyType.Nullable(ret);
            }

            return ret;
        }

        public static string EmmyName(this ICustomAttributeProvider p, string nameSimple, bool camelCase = false)
        {
            if (camelCase)
            {
                nameSimple = nameSimple.ToCamelCase();
            }

            var name = p.EmmyAlias() ?? nameSimple;

            return RenameAvoidKeyword(name);
        }

        public static bool Visible(this ICustomAttributeProvider p)
        {
            var isVisible = true;

            if (p is Type)
            {
                isVisible = isVisible
                    && p.GetAttrOrNull<MoonSharpUserDataAttribute>() != null;
            }
            else if (p is MethodInfo method)
            {
                var baseMethod = method.GetBaseDefinition();

                isVisible = isVisible
                    && (method == baseMethod || baseMethod.Visible());
            }
            else
            {
                List<Type> types = new List<Type>();

                if (p is FieldInfo field)
                {
                    types.Add(field.FieldType);
                }
                else if (p is PropertyInfo prop)
                {
                    types.Add(prop.PropertyType);
                }
                else if (p is MethodInfo method2)
                {
                    types.Add(method2.ReturnType);
                    types.AddRange(method2.GetParameters().Select(param => param.ParameterType));
                }
                else if (p is ParameterInfo param)
                {
                    types.Add(param.ParameterType);
                }

                foreach (var t in types)
                {
                    isVisible = isVisible
                        && EmmyType.From(t) != null;
                }
            }

            isVisible = isVisible
                && p.GetAttrOrNull<MoonSharpHiddenAttribute>() == null
                && p.GetAttrOrNull<MoonSharpHideMemberAttribute>() == null;

            return isVisible;
        }

        public static string EmmyName<T>(this T p, bool camelCase = false)
            where T : MemberInfo, ICustomAttributeProvider
            => p.EmmyName(p.Name, camelCase);

        public static string EmmyAlias(this ICustomAttributeProvider p)
            => p.GetAttrOrNull<EmmyAliasAttribute>()?.Alias
            ?? p.GetAttrOrNull<MoonSharpUserDataMetamethodAttribute>()?.Name;

        private static string RenameAvoidKeyword(string name)
        {
            switch (name)
            {
                case "and":
                case "break":
                case "do":
                case "else":
                case "elseif":
                case "end":
                case "false":
                case "for":
                case "function":
                case "goto":
                case "if":
                case "in":
                case "local":
                case "nil":
                case "not":
                case "or":
                case "repeat":
                case "return":
                case "then":
                case "true":
                case "until":
                case "while":
                    return "_" + name;
                default:
                    return name;
            }
        }

        private static readonly ISet<string> LuaOperators = new HashSet<string>
        {
            "__eq",
            "__lt",
            "__le",

            "__unm",
            "__add",
            "__sub",
            "__div",
            "__mul",
            "__mod",
            "__pow",
            "__concat",

            "__tostring",
        };

        private static readonly IReadOnlyDictionary<string, string> OperatorMappings = new Dictionary<string, string>
        {
            { "op_Equality", "__eq" },
            { "op_LessThan", "__lt" },
            { "op_LessThanOrEqual", "__le" },
            { "op_UnaryNegation", "__unm" },
            { "op_Addition", "__add" },
            { "op_Subtraction", "__sub" },
            { "op_Multiply", "__mul" },
            { "op_Division", "__div" },
            { "op_Modulus", "__mod" },
        };

        public static string GetLuaOperator(string csOp)
        {
            if (!OperatorMappings.TryGetValue(csOp, out var luaOp))
            {
                return null;
            }

            return luaOp;
        }

        public static bool IsLuaOperator(string luaOp)
            => LuaOperators.Contains(luaOp);
    }
}