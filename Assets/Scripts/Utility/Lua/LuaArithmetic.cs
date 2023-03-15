using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.BasicDescriptors;

namespace ArcCreate.Utility.Lua
{
    public static class LuaArithmetic
    {
        public static void SetupForBaseType<T>()
            where T : class
        {
            var descr = (StandardUserDataDescriptor)UserData.RegisterType<T>();

            Dictionary<string, IMemberDescriptor> operatorOverloadDescriptors = new Dictionary<string, IMemberDescriptor>();
            foreach (var pair in descr.Members)
            {
                if (pair.Value is OverloadedMethodMemberDescriptor && pair.Key.StartsWith("op_"))
                {
                    operatorOverloadDescriptors.Add(pair.Key, pair.Value);
                }
            }

            var arithmeticTypes = Assembly.GetAssembly(typeof(T)).GetTypes().Where(type => type.IsSubclassOf(typeof(T)));
            foreach (var arithmeticType in arithmeticTypes)
            {
                var arithDescr = (StandardUserDataDescriptor)UserData.RegisterType(arithmeticType);
                foreach (var pair in operatorOverloadDescriptors)
                {
                    arithDescr.AddMember(pair.Key, pair.Value);
                }
            }
        }
    }
}