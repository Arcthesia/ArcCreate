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

namespace EmmySharp
{
    public class EmmySharpBuilder
    {
        private readonly StringBuilder builder = new StringBuilder();

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
            => builder.ToString();

        /// <summary>
        /// Append documentation. If null is provided, do nothing.
        /// </summary>
        /// <param name="doc">The document to append.</param>
        /// <returns>The builder instance.</returns>
        public EmmySharpBuilder AppendDoc(string doc)
        {
            if (doc is null)
            {
                return this;
            }

            foreach (var line in doc.Split(Environment.NewLine.ToCharArray()))
            {
                builder.AppendLine("---" + line);
            }

            return this;
        }

        /// <summary>
        /// Append a static value specification to this builder.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <param name="baseTy">The base type.</param>
        /// <param name="alias">The base type's alias.</param>
        /// <returns>The builder instance.</returns>
        public EmmySharpBuilder AppendStaticValue(EmmySharpValue value, Type baseTy = null, string alias = null)
        {
            AppendDoc(value.Doc);
            builder.Append("---@type ");
            AppendTypeName(value.Type, value.Options);
            builder.AppendLine();

            if (baseTy != null)
            {
                UnityEngine.Debug.Log(alias + " " + baseTy.Name + " " + (alias ?? baseTy.Name));
                builder.Append((alias ?? baseTy.Name) + ".");
            }

            builder.AppendLine(value.Name.ToCamelCase() + " = nil");
            builder.AppendLine();

            return this;
        }

        /// <summary>
        /// Append a field specification to this builder. Should only be called while defining a class.
        /// </summary>
        /// <param name="field">The field to append.</param>
        /// <returns>The builder instance.</returns>
        public EmmySharpBuilder AppendField(EmmySharpValue field)
        {
            AppendDoc(field.Doc);
            builder.Append("---@field public " + field.Name.ToCamelCase() + " ");
            AppendTypeName(field.Type, field.Options);
            builder.AppendLine();

            return this;
        }

        /// <summary>
        /// Append a class definition (static or otherwise) to this builder.
        /// </summary>
        /// <param name="type">The class type to append.</param>
        /// <param name="values">List of values.</param>
        /// <returns>The builder instance.</returns>
        public EmmySharpBuilder AppendClassDefinition(Type type, IEnumerable<EmmySharpValue> values)
        {
            AppendDoc(type.EmmyDoc());
            string alias = type.EmmyAlias();
            builder
                .AppendLine($"{alias ?? type.Name} = {{}}")
                .AppendLine();

            var staticValues = values.Where(f => f.IsStatic).ToArray();

            foreach (var staticVal in staticValues)
            {
                AppendStaticValue(staticVal, type, alias);
            }

            bool singleton = type.IsDefined(typeof(EmmySingletonAttribute));

            // Ignore instance values and table for static classes
            if (!type.IsAbstract || !type.IsSealed)
            {
                AppendDoc(type.EmmyDoc());
                builder.Append($"---@class {alias ?? type.Name}");

                if (type.BaseType != typeof(object) && Attribute.IsDefined(type.BaseType, typeof(MoonSharpUserDataAttribute)))
                {
                    string baseAlias = type.BaseType.EmmyAlias();
                    builder.Append($" : {baseAlias ?? type.BaseType.Name}");
                }

                builder.AppendLine();

                foreach (var field in values.Where(f => !f.IsStatic))
                {
                    AppendField(field);
                }

                if (!singleton)
                {
                    builder.AppendLine($"{alias ?? type.Name}__inst = {{}}");
                    builder.AppendLine();
                }
            }
            else if (staticValues.Length != 0)
            {
                builder.AppendLine();
            }

            return this;
        }

        /// <summary>
        /// Append a function with the given documentation. If this function belongs to a type,
        /// pass it as the member type.
        /// </summary>
        /// <param name="method">The method to append.</param>
        /// <param name="memberType">The member type.</param>
        /// <returns>The builder instance.</returns>
        public EmmySharpBuilder AppendFunction(MethodInfo method, Type memberType = null)
        {
            AppendDoc(method.EmmyDoc());

            var parameters = method.GetParameters();

            string deprecationNotice = method.EmmyDeprecated();
            if (method.EmmyDeprecated()!=null)
            {
                builder.Append($"---@deprecated {deprecationNotice}");
                builder.AppendLine();
            }
            foreach (var p in parameters)
            {
                // MoonSharp automatically injects context when there's Script in the parameter, ignore it.
                // https://www.moonsharp.org/callback.html#returning-a-table (see: There are two things to notice...)
                if (p.ParameterType == typeof(Script))
                {
                    continue;
                }
                builder.Append($"---@param {p.Name} ");
                AppendTypeName(p.ParameterType, p.EmmyChoice());
                builder.AppendLine();
            }

            if (method.ReturnType != typeof(void))
            {
                builder.Append($"---@return ");
                AppendTypeName(method.ReturnType, method.ReturnTypeCustomAttributes.EmmyChoice());
                builder.AppendLine();
            }

            builder.Append("function ");

            if (memberType != null)
            {
                string memberTypeAlias = memberType.EmmyAlias();
                builder.Append(memberTypeAlias ?? memberType.Name);

                bool singleton = memberType.IsDefined(typeof(EmmySingletonAttribute));
                if (!method.IsStatic && !singleton)
                {
                    builder.Append("__inst");
                }

                builder.Append('.');
            }

            string alias = method.EmmyAlias();
            builder.Append(alias ?? method.Name.ToCamelCase()).Append('(');

            for (var i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                if (p.ParameterType == typeof(Script))
                {
                    continue;
                }
                string paramName = RenameAvoidKeyword(p.Name);

                builder.Append(paramName);
                if (i != parameters.Length - 1)
                {
                    builder.Append(", ");
                }
            }

            builder
                .AppendLine(") end")
                .AppendLine();

            return this;
        }

        /// <summary>
        /// Appends the type name of the given type.
        /// This function may 'error out', in which case an erroneous (but still legal)
        /// type will be appended and a warning printed to the console's standard error.
        /// </summary>
        /// <param name="ty">Type to append.</param>
        /// <param name="options">List of options.</param>
        /// <returns>The builder instance.</returns>
        public EmmySharpBuilder AppendTypeName(Type ty, string[] options = null)
        {
            if (ty == typeof(short) || ty == typeof(int) || ty == typeof(long)
            || ty == typeof(ushort) || ty == typeof(uint) || ty == typeof(ulong))
            {
                builder.Append("integer");
            }
            else if (ty == typeof(float) || ty == typeof(double) || ty == typeof(decimal))
            {
                builder.Append("number");
            }
            else if (ty == typeof(bool))
            {
                builder.Append("boolean");
            }
            else if (ty == typeof(string))
            {
                if (options != null)
                {
                    builder
                        .Append('(')
                        .Append(string.Join(" | ", options.Select(t => $"'{t}'")))
                        .Append(')');
                }
                else
                {
                    builder.Append("string");
                }
            }
            else if (ty == typeof(object))
            {
                builder.Append("any");
            }
            else if (ty.IsArray)
            {
                AppendTypeName(ty.GetElementType(), options);
                builder.Append("[]");
            }
            else if (ty.IsGenericType && ty.GetGenericTypeDefinition() == typeof(List<>))
            {
                AppendTypeName(ty.GetGenericArguments()[0], options);
                builder.Append("[]");
            }
            else if (ty.IsGenericType && ty.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                builder.Append("table<");
                AppendTypeName(ty.GetGenericArguments()[0], options);
                builder.Append(", ");
                AppendTypeName(ty.GetGenericArguments()[1], options);
                builder.Append('>');
            }
            else if (Attribute.IsDefined(ty, typeof(MoonSharpUserDataAttribute)))
            {
                builder.Append(ty.Name);
            }
            else if (typeof(Delegate).IsAssignableFrom(ty))
            {
                var invoke = ty.GetMethod("Invoke");
                var iparams = invoke.GetParameters();

                builder.Append("fun(");
                for (var i = 0; i < iparams.Length; i++)
                {
                    var p = iparams[i];

                    if (p.ParameterType == ty)
                    {
                        Console.Error.WriteLine($"[ERROR]: Cannot generate emmylua for type {ty.FullName} since it is a recursive delegate type, falling back to 'fun(...):any'");
                        builder.Append(") : any");
                        return this;
                    }

                    builder.Append(p.Name + ":");
                    AppendTypeName(p.ParameterType, p.EmmyChoice());

                    if (i != iparams.Length - 1)
                    {
                        builder.Append(", ");
                    }
                }

                builder.Append(')');

                if (invoke.ReturnType != typeof(void))
                {
                    builder.Append(" : ");
                    AppendTypeName(invoke.ReturnType, options);
                }
            }
            else
            {
                Console.Error.WriteLine($"[ERROR]: Cannot generate emmylua for type {ty.FullName}, falling back to type 'any'");
                builder.Append("any");
            }

            return this;
        }

        /// <summary>
        /// Append the given type as if it were exposed by MoonSharp.
        /// </summary>
        /// <param name="type">The type to append.</param>
        /// <returns>The builder instance.</returns>
        public EmmySharpBuilder AppendType(Type type)
        {
            var fields = new List<EmmySharpValue>();

            foreach (var val in type.GetFields()
                .Where(t => t.IsPublic)
                .Where(t => t.DeclaringType == type)
                .Where(t => !Attribute.IsDefined(t, typeof(MoonSharpHiddenAttribute))))
            {
                fields.Add(new EmmySharpValue(val.EmmyDoc(), val.Name, val.FieldType, val.EmmyChoice(), val.IsStatic));
            }

            foreach (var val in type.GetProperties()
                .Where(t => t.GetAccessors().Any(a => a.IsPublic))
                .Where(t => t.DeclaringType == type)
                .Where(t => !Attribute.IsDefined(t, typeof(MoonSharpHiddenAttribute))))
            {
                fields.Add(new EmmySharpValue(val.EmmyDoc(), val.Name, val.PropertyType, val.EmmyChoice(), val.GetGetMethod().IsStatic));
            }

            AppendClassDefinition(type, fields);

            foreach (var met in type.GetMethods()
                .Where(t => t.IsPublic)
                .Where(t => !t.IsSpecialName)
                .Where(t => t.DeclaringType == type)
                .Where(t => !Attribute.IsDefined(t, typeof(MoonSharpHiddenAttribute))))
            {
                AppendFunction(met, type);
            }

            return this;
        }

        /// <summary>
        /// Append the given type as if it were exposed by MoonSharp.
        /// </summary>
        /// <typeparam name="T">The type to append.</typeparam>
        /// <returns>The builder instance.</returns>
        public EmmySharpBuilder AppendType<T>()
            => AppendType(typeof(T));

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
                .Where(t => Attribute.IsDefined(t, typeof(EmmyGroupAttribute)))
                .Where(t => ((EmmyGroupAttribute)Attribute.GetCustomAttribute(t, typeof(EmmyGroupAttribute))).GroupName == group))
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
            foreach (var ty in assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(MoonSharpUserDataAttribute))))
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
        }

        private string RenameAvoidKeyword(string name)
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
    }
}