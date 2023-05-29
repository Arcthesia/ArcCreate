/// <summary>
/// EmmySharp, created by Dylan Rafael (floofer++) for use by 0thElement.
/// The below classes help to generate EmmyLua workspace files from MoonSharp classes.
/// </summary>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter;
using UnityEngine;

namespace EmmySharp
{
    public abstract class EmmyType
    {
        public virtual IEnumerable<EmmyType> Constituents
        {
            get
            {
                yield return this;
            }
        }

        public static EmmyType From<T>()
            => From(typeof(T));

        public static EmmyType Void
            => Raw("nil");

        public static EmmyType Integer
            => Raw("integer");

        public static EmmyType Float
            => Raw("number");

        public static EmmyType Bool
            => Raw("boolean");

        public static EmmyType String
            => Raw("string");

        public static EmmyType Any
            => Raw("any");

        public static EmmyType From(Type ty)
        {
            if (ty == typeof(void))
            {
                return Void;
            }
            else if (ty == typeof(short) || ty == typeof(int) || ty == typeof(long)
            || ty == typeof(ushort) || ty == typeof(uint) || ty == typeof(ulong))
            {
                return Integer;
            }
            else if (ty == typeof(float) || ty == typeof(double) || ty == typeof(decimal))
            {
                return Float;
            }
            else if (ty == typeof(bool))
            {
                return Bool;
            }
            else if (ty == typeof(string))
            {
                return String;
            }
            else if (ty == typeof(object) || ty == typeof(DynValue))
            {
                return Any;
            }
            else if (ty.IsArray)
            {
                var inner = From(ty.GetElementType());
                return inner == null ? null : Array(inner);
            }
            else if (ty.IsGenericType && ty.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var inner = From(ty.GetGenericArguments()[0]);
                return inner == null ? null : Nullable(inner);
            }
            else if (ty.IsGenericType && ty.GetGenericTypeDefinition() == typeof(List<>))
            {
                var inner = From(ty.GetGenericArguments()[0]);
                return inner == null ? null : Array(inner);
            }
            else if (ty.IsGenericType && ty.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var key = From(ty.GetGenericArguments()[0]);
                var value = From(ty.GetGenericArguments()[1]);

                if (key == null || value == null)
                {
                    return null;
                }

                return Table(key, value);
            }
            else if (typeof(Delegate).IsAssignableFrom(ty))
            {
                var invoke = ty.GetMethod("Invoke");
                var iparams = invoke.GetParameters();

                if (iparams.Any(p => p.ParameterType == ty))
                {
                    throw new InvalidOperationException("Cannot create a function type which is self-referential!");
                }

                var ret = From(invoke.ReturnType);
                var par = iparams.Select(p => new EmmyValue { Type = From(p.ParameterType), Name = p.Name })
                    .ToArray();

                if (ret == null || par.Any(p => p.Type == null))
                {
                    return null;
                }

                return Function(ret, par);
            }
            else if (ty.Visible())
            {
                return new WrapperType { Type = ty };
            }

            return null;
        }

        public static EmmyType Array(EmmyType inner)
        {
            return new ArrayType { Inner = inner };
        }

        public static EmmyType Nullable(EmmyType inner)
        {
            return new NullableType { Inner = inner };
        }

        public static EmmyType Table(EmmyType key, EmmyType value)
        {
            return new TableType { Key = key, Value = value };
        }

        public static EmmyType Table(params (string name, EmmyType type)[] fields)
        {
            return new TableLiteralType { Fields = fields.ToDictionary(p => p.name, p => p.type) };
        }

        public static EmmyType Alias(string name, EmmyType inner)
        {
            return new AliasType { Name = name, Inner = inner };
        }

        public static EmmyType Option(params EmmyType[] types)
        {
            return new OptionType { Options = types };
        }

        public static EmmyType Option(params string[] literals)
        {
            return Option(literals.Select(Literal).ToArray());
        }

        public static EmmyType Function(EmmyType ret, params EmmyValue[] parameters)
        {
            return new FunctionType { Return = ret, Parameters = parameters };
        }

        public static EmmyType Function(params EmmyValue[] parameters)
        {
            return new FunctionType { Return = null, Parameters = parameters };
        }

        public static EmmyType Literal(string s)
        {
            return new LiteralStringType { Value = s };
        }

        public static EmmyType Raw(string s)
        {
            return new RawType { Text = s };
        }

        public virtual string GetDefinition()
            => string.Empty;

        public abstract override string ToString();

        private class WrapperType : EmmyType
        {
            public Type Type { get; set; }

            public override string ToString()
            {
                return Type.Name;
            }
        }

        private class AliasType : EmmyType
        {
            public string Name { get; set; }

            public EmmyType Inner { get; set; }

            public override IEnumerable<EmmyType> Constituents
            {
                get
                {
                    yield return this;
                    yield return Inner;
                }
            }

            public override string GetDefinition()
            {
                return "---@alias " + Name + " " + Inner.ToString();
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private class NullableType : EmmyType
        {
            public EmmyType Inner { get; set; }

            public override IEnumerable<EmmyType> Constituents
            {
                get
                {
                    yield return Inner;
                }
            }

            public override string ToString()
            {
                return Inner.ToString() + "?";
            }
        }

        private class ArrayType : EmmyType
        {
            public EmmyType Inner { get; set; }

            public override IEnumerable<EmmyType> Constituents
            {
                get
                {
                    yield return Inner;
                }
            }

            public override string ToString()
            {
                return Inner.ToString() + "[]";
            }
        }

        private class OptionType : EmmyType
        {
            public IReadOnlyList<EmmyType> Options { get; set; }

            public override IEnumerable<EmmyType> Constituents
            {
                get
                {
                    foreach (var item in Options)
                    {
                        yield return item;
                    }
                }
            }

            public override string ToString()
            {
                return "(" + string.Join(" | ", Options) + ")";
            }
        }

        private class LiteralStringType : EmmyType
        {
            public string Value { get; set; }

            public override string ToString()
            {
                return $"\"{Value}\"";
            }
        }

        private class RawType : EmmyType
        {
            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        private class TableLiteralType : EmmyType
        {
            public override IEnumerable<EmmyType> Constituents
            {
                get
                {
                    foreach (var f in Fields.Values)
                    {
                        yield return f;
                    }
                }
            }

            public IReadOnlyDictionary<string, EmmyType> Fields { get; set; }

            public override string ToString()
            {
                return $"{{{string.Join(", ", Fields.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}}}";
            }
        }

        private class TableType : EmmyType
        {
            public override IEnumerable<EmmyType> Constituents
            {
                get
                {
                    yield return Key;
                    yield return Value;
                }
            }

            public EmmyType Key { get; set; }

            public EmmyType Value { get; set; }

            public override string ToString()
            {
                return $"table<{Key}, {Value}>";
            }
        }

        private class FunctionType : EmmyType
        {
            public override IEnumerable<EmmyType> Constituents
            {
                get
                {
                    foreach (var v in Parameters)
                    {
                        yield return v.Type;
                    }

                    yield return Return;
                }
            }

            public IReadOnlyList<EmmyValue> Parameters { get; set; }

            public EmmyType Return { get; set; }

            public override string ToString()
            {
                var ret = "fun(" + string.Join(", ", Parameters.Select(p => p.Name + ": " + p.Type.ToString())) + ")";
                if (Return != null)
                {
                    ret += " : " + Return.ToString();
                }

                return ret;
            }
        }
    }

    public class EmmyValue
    {
        public string Name { get; set; }

        public string Doc { get; set; }

        public virtual EmmyType Type { get; set; }

        public bool ParamHasDefaultValue { get; set; }
    }

    public class EmmyFunction : EmmyValue
    {
        public IReadOnlyList<EmmyValue> Parameters { get; set; }

        public EmmyType Return { get; set; }

        public override EmmyType Type
        {
            get => EmmyType.Function(Return, Parameters.ToArray());
            set => throw new InvalidOperationException();
        }
    }

    public class EmmyClass
    {
        public string Doc { get; set; }

        public string Name { get; set; }

        public bool IsStatic { get; set; }

        public bool IsSingleton { get; set; }

        public EmmyClass Base { get; set; }

        public IReadOnlyList<EmmyValue> Members { get; set; }

        public IReadOnlyList<EmmyValue> Statics { get; set; }
    }

    public class EmmySharpBuilder
    {
        private readonly StringBuilder outputBuilder = new StringBuilder();

        private readonly HashSet<EmmyType> constituentTypes = new HashSet<EmmyType>();
        private readonly Dictionary<Type, EmmyClass> classes = new Dictionary<Type, EmmyClass>();
        private readonly List<EmmyValue> values = new List<EmmyValue>();

        private readonly Dictionary<string, EmmyType> aliases = new Dictionary<string, EmmyType>();

        private void Add(EmmyValue value)
        {
            values.Add(value);

            foreach (var t in value.Type.Constituents)
            {
                constituentTypes.Add(t);
            }
        }

        private void Add(Type source, EmmyClass classs)
        {
            classes[source] = classs;

            foreach (var v in classs.Members.Concat(classs.Statics))
            {
                foreach (var t in v.Type.Constituents)
                {
                    constituentTypes.Add(t);
                }
            }
        }

        /// <summary>
        /// Get an emmy sharp builder which contains type information about the
        /// calling assembly exposed through MoonSharp.
        /// </summary>
        /// <returns>An builder instance.</returns>
        public static EmmySharpBuilder ForThisAssembly()
        {
            var builder = new EmmySharpBuilder();

            builder.AppendAssembly(Assembly.GetCallingAssembly());

            return builder;
        }

        /// <summary>
        /// Get the contents of this builder so far.
        /// </summary>
        /// <returns>The content string.</returns>
        public override string ToString()
        {
            outputBuilder.Clear();
            outputBuilder.AppendLine("---@meta").AppendLine();

            foreach (var consType in constituentTypes)
            {
                var definition = consType.GetDefinition();

                if (!string.IsNullOrEmpty(definition))
                {
                    outputBuilder.AppendLine(definition).AppendLine();
                }
            }

            foreach (var freeValue in values)
            {
                RenderValue(freeValue);
            }

            foreach (var cls in classes.Values)
            {
                RenderClass(cls);
            }

            return outputBuilder.ToString();
        }

        /// <summary>
        /// Append documentation. If null is provided, do nothing.
        /// </summary>
        /// <param name="doc">The document to append.</param>
        /// <returns>The builder instance.</returns>
        private EmmySharpBuilder RenderDoc(string doc)
        {
            if (doc is null)
            {
                return this;
            }

            foreach (var line in doc.Split(Environment.NewLine.ToCharArray()))
            {
                outputBuilder.AppendLine("---" + line);
            }

            return this;
        }

        /// <summary>
        /// Append a static value specification to this builder.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <param name="baseV">The base value.</param>
        /// <returns>The builder instance.</returns>
        private EmmySharpBuilder RenderValue(EmmyValue value, string baseV = null)
        {
            if (value is EmmyFunction f)
            {
                return RenderFunction(f, baseV);
            }

            RenderDoc(value.Doc);
            outputBuilder.Append("---@type ");
            outputBuilder.Append(value.Type);
            outputBuilder.AppendLine();

            if (baseV != null)
            {
                outputBuilder.Append(baseV + ".");
            }

            outputBuilder.AppendLine(value.Name + " = nil");
            outputBuilder.AppendLine();

            return this;
        }

        /// <summary>
        /// Append a field specification to this builder. Should only be called while defining a class.
        /// </summary>
        /// <param name="field">The field to append.</param>
        /// <returns>The builder instance.</returns>
        private EmmySharpBuilder RenderField(EmmyValue field)
        {
            outputBuilder.Append("---@field public " + field.Name + " ");
            outputBuilder.Append(field.Type);
            outputBuilder.Append(' ');
            outputBuilder.Append(field.Doc?.Replace(Environment.NewLine, "\n"));
            outputBuilder.AppendLine();

            return this;
        }

        /// <summary>
        /// Append a class definition (static or otherwise) to this builder.
        /// </summary>
        /// <returns>The builder instance.</returns>
        private EmmySharpBuilder RenderClass(EmmyClass cls)
        {
            RenderDoc(cls.Doc);
            outputBuilder
                .AppendLine($"{cls.Name} = {{}}")
                .AppendLine();

            foreach (var staticVal in cls.IsSingleton ? cls.Statics.Concat(cls.Members) : cls.Statics)
            {
                RenderValue(staticVal, cls.Name);
            }

            // Ignore instance values and table for static classes
            if (!cls.IsStatic && !cls.IsSingleton)
            {
                RenderDoc(cls.Doc);
                outputBuilder.Append($"---@class {cls.Name}");

                if (cls.Base != null)
                {
                    outputBuilder.Append($" : {cls.Base.Name}");
                }

                outputBuilder.AppendLine();

                foreach (var field in cls.Members)
                {
                    if (!(field is EmmyFunction))
                    {
                        RenderField(field);
                    }
                }

                outputBuilder.AppendLine($"local {cls.Name}__inst = {{}}");
                outputBuilder.AppendLine();

                foreach (var field in cls.Members)
                {
                    if (field is EmmyFunction f)
                    {
                        RenderFunction(f, $"{cls.Name}__inst");
                    }
                }
            }

            return this;
        }

        private EmmyFunction LoadMethod(MethodInfo met)
        {
            return new EmmyFunction
            {
                Doc = met.EmmyDoc(),
                Name = met.EmmyName(camelCase: true),
                Parameters = met.GetParameters()
                    .Select(p => new EmmyValue { Doc = p.EmmyDoc(), Name = p.EmmyName(p.Name), Type = p.GetEmmyType(p.ParameterType, aliases), ParamHasDefaultValue = p.HasDefaultValue })
                    .ToArray(),
                Return = met.ReturnParameter.GetEmmyType(met.ReturnType, aliases),
            };
        }

        public EmmySharpBuilder AppendFunction(MethodInfo met)
        {
            Add(LoadMethod(met));

            return this;
        }

        public EmmySharpBuilder AppendFunction<T>(string name)
            => AppendFunction(typeof(T).GetMethod(name));

        /// <summary>
        /// Append a function with the given documentation. If this function belongs to a type,
        /// pass it as the member type.
        /// </summary>
        /// <returns>The builder instance.</returns>
        private EmmySharpBuilder RenderFunction(EmmyFunction function, string baseV = "")
        {
            RenderDoc(function.Doc);

            foreach (var p in function.Parameters)
            {
                outputBuilder.Append($"---@param {p.Name}{(p.ParamHasDefaultValue ? "?" : "")} ");
                outputBuilder.Append(p.Type);
                outputBuilder.Append(' ');
                outputBuilder.Append(p.Doc);
                outputBuilder.AppendLine();
            }

            if (function.Return != null)
            {
                outputBuilder.Append($"---@return ");
                outputBuilder.Append(function.Return);
                outputBuilder.AppendLine();
            }

            outputBuilder.Append("function ");

            if (!string.IsNullOrEmpty(baseV))
            {
                outputBuilder.Append(baseV);
                outputBuilder.Append('.');
            }

            outputBuilder.Append(function.Name).Append('(');

            for (var i = 0; i < function.Parameters.Count; i++)
            {
                var p = function.Parameters[i];

                outputBuilder.Append(p.Name);
                if (i != function.Parameters.Count - 1)
                {
                    outputBuilder.Append(", ");
                }
            }

            outputBuilder
                .AppendLine(") end")
                .AppendLine();

            return this;
        }

        /// <summary>
        /// Append the given type as if it were exposed by MoonSharp.
        /// </summary>
        /// <param name="type">The type to append.</param>
        /// <returns>The builder instance.</returns>
        public EmmySharpBuilder AppendType(Type type)
        {
            EmmyClass BuildClass(Type ty)
            {
                if (ty.IsConstructedGenericType)
                {
                    ty = ty.GetGenericTypeDefinition();
                }

                var cls = new EmmyClass { Name = ty.EmmyName() };

                if (ty.BaseType != null && ty.BaseType.Visible())
                {
                    var baseTy = ty.BaseType;

                    if (baseTy.IsConstructedGenericType)
                    {
                        // TODO: support real generics
                        baseTy = baseTy.GetGenericTypeDefinition();
                    }

                    if (!classes.TryGetValue(ty.BaseType, out var baseCls))
                    {
                        baseCls = BuildClass(ty.BaseType);
                    }

                    cls.Base = baseCls;
                }

                var members = new List<EmmyValue>();
                var statics = new List<EmmyValue>();

                cls.Members = members;
                cls.Statics = statics;

                foreach (var val in ty.GetFields()
                    .Where(t => t.IsPublic)
                    .Where(t => t.DeclaringType == ty)
                    .Where(EmmyHelpers.Visible))
                {
                    (val.IsStatic ? statics : members).Add(new EmmyValue
                    {
                        Doc = val.EmmyDoc(),
                        Name = val.EmmyName(camelCase: true),
                        Type = val.GetEmmyType(val.FieldType, aliases),
                    });
                }

                foreach (var val in ty.GetProperties()
                    .Where(t => t.GetAccessors().First().IsPublic)
                    .Where(t => t.DeclaringType == ty)
                    .Where(EmmyHelpers.Visible))
                {
                    (val.GetAccessors().First().IsStatic ? statics : members).Add(new EmmyValue
                    {
                        Doc = val.EmmyDoc(),
                        Name = val.EmmyName(camelCase: true),
                        Type = val.GetEmmyType(val.PropertyType, aliases),
                    });
                }

                foreach (var met in ty.GetMethods()
                    .Where(t => t.IsPublic)
                    .Where(t => !t.IsSpecialName)
                    .Where(t => t.DeclaringType == ty)
                    .Where(EmmyHelpers.Visible))
                {
                    // TODO: handle operators
                    (met.IsStatic ? statics : members).Add(LoadMethod(met));
                }

                cls.IsStatic = ty.IsAbstract && ty.IsSealed;
                cls.IsSingleton = ty.GetAttrOrNull<EmmySingletonAttribute>() != null;

                Add(ty, cls);

                return cls;
            }

            BuildClass(type);
            return this;
        }

        /// <summary>
        /// Append the given type as if it were exposed by MoonSharp.
        /// </summary>
        /// <typeparam name="T">The type to append.</typeparam>
        /// <returns>The builder instance.</returns>
        public EmmySharpBuilder AppendType<T>()
            => AppendType(typeof(T));

        public EmmySharpBuilder AppendAlias(string name, EmmyType type)
        {
            var alias = EmmyType.Alias(name, type);
            aliases[name] = alias;

            foreach (var constituent in alias.Constituents)
            {
                constituentTypes.Add(constituent);
            }

            return this;
        }

        /// <summary>
        /// Append all type information from the given assembly, including types not added to MoonSharp,
        /// which fall under the given group.
        /// </summary>
        /// <param name="assembly">Assembly that contains the classes.</param>
        /// <param name="group">Group name to append.</param>
        /// <returns>The builder instance.</returns>
        public EmmySharpBuilder AppendGroup(Assembly assembly, string group)
        {
            foreach (var ty in assembly.GetTypes()
                .Where(EmmyHelpers.Visible)
                .Where(t => t.GetAttrOrNull<EmmyGroupAttribute>()?.GroupName == group))
            {
                AppendType(ty);
            }

            return this;
        }

        /// <summary>
        /// Append all type information from the calling assembly, including types not added to MoonSharp,
        /// which fall under the given group.
        /// </summary>
        /// <param name="group">Group name to append.</param>
        /// <returns>The builder instance.</returns>
        public EmmySharpBuilder AppendGroup(string group)
            => AppendGroup(Assembly.GetCallingAssembly(), group);

        /// <summary>
        /// Append all type information given by the provided assembly exposed through MoonSharp.
        /// </summary>
        /// <param name="assembly">Assembly that contains the classes.</param>
        /// <returns>The builder instance.</returns>
        public EmmySharpBuilder AppendAssembly(Assembly assembly)
        {
            foreach (var ty in assembly.GetTypes().Where(EmmyHelpers.Visible))
            {
                AppendType(ty);
            }

            return this;
        }

        /// <summary>
        /// Put the contents of this builder into '{filepath}/.vscode/workspace/lib.lua'.
        /// </summary>
        /// <param name="filepath">The output file path.</param>
        public void Build(string filepath)
        {
            var c = ToString();

            filepath = Path.Combine(filepath, ".vscode");
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }

            filepath = Path.Combine(filepath, "workspace");
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }

            filepath = Path.Combine(filepath, "lib.lua");
            File.WriteAllText(filepath, c);

            Debug.Log("Built emmylua to " + filepath);
        }
    }
}