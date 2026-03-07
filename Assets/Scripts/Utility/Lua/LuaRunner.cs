using System;
using System.IO;
using EmmySharp;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using MoonSharp.VsCodeDebugger;
using UnityEngine;

namespace ArcCreate.Utility.Lua
{
    public static class LuaRunner
    {
        private static bool hasRegisteredAssembly = false;

        /// <summary>
        /// Runs a string of lua script.
        /// </summary>
        /// <param name="script">The script to run.</param>
        /// <param name="setup">The object to set up the script, for binding methods.</param>
        /// <param name="scriptLoader">The script loader instance for loading script from file system.</param>
        /// <param name="scriptFileName">The filename of the script to be run.</param>
        /// <param name="scriptBasePath">The dir path to the script.</param>
        /// <param name="debugServer">MoonSharp debugger instance, leave it null to disable</param>
        public static Script RunScript(
            string script,
            IScriptSetup setup,
            FileSystemScriptLoader scriptLoader = null,
            [CanBeNull] string scriptFileName = null,
            [CanBeNull] string scriptBasePath = null,
            [CanBeNull] MoonSharpVsCodeDebugServer debugServer = null)
        {
            Script scriptObject = new Script();
            Script.GlobalOptions.RethrowExceptionNested = true;
            scriptObject.Options.UseLuaErrorLocations = true;

            if (!hasRegisteredAssembly)
            {
                UserData.RegisterAssembly();
                hasRegisteredAssembly = true;
            }

            if (scriptLoader != null)
            {
                scriptObject.Options.ScriptLoader = scriptLoader;
            }

            RegisterCommon(scriptObject);
            setup.SetupScript(scriptObject);

            if (debugServer != null && scriptFileName != null && scriptBasePath != null)
            {
                debugServer.AttachToScript(scriptObject, scriptFileName,
                    s => Path.Combine(scriptBasePath, s.Name.Replace('/', Path.DirectorySeparatorChar)));
            }

            scriptObject.DoString(script, null, scriptFileName);

            return scriptObject;
        }

        public static void RegisterCommon(Script scriptObject)
        {
            scriptObject.Globals["xy"] = (Func<float, float, XY>)XY;
            scriptObject.Globals["xyz"] = (Func<float, float, float, XYZ>)XYZ;
            scriptObject.Globals["hsva"] = (Func<float, float, float, float, HSVA>)HSVA;
            scriptObject.Globals["rgba"] = (Func<float, float, float, float, RGBA>)RGBA;
            scriptObject.Globals["Convert"] = new Convert();

            scriptObject.Globals["log"] = (Action<object>)Log;
            scriptObject.Options.DebugPrint = Log; // re-direct lua `print` function
            
            scriptObject.Globals["toNumber"] = (Func<DynValue, double>)ToNumber;
            scriptObject.Globals["toBool"] = (Func<DynValue, bool>)ToBool;
        }

        [EmmyAlias("xy")]
        public static XY XY(float x, float y) => new XY(x, y);

        [EmmyAlias("xyz")]
        public static XYZ XYZ(float x, float y, float z) => new XYZ(x, y, z);

        [EmmyAlias("hsva")]
        public static HSVA HSVA(float h, float s, float v, float a) => new HSVA(h, s, v, a);

        [EmmyAlias("rgba")]
        public static RGBA RGBA(float r, float g, float b, float a) => new RGBA(r, g, b, a);

        public static void Log(object content) => Debug.Log(content.ToString());

        public static double ToNumber(DynValue value)
        {
            if (value.Type == DataType.Number)
            {
                return value.Number;
            }

            if (double.TryParse(value.String, out double result))
            {
                return result;
            }

            return 0;
        }

        public static bool ToBool(DynValue value)
        {
            if (value.Type == DataType.Boolean)
            {
                return value.Boolean;
            }

            if (bool.TryParse(value.String.ToLower(), out bool result))
            {
                return result;
            }

            return false;
        }

        public static EmmySharpBuilder GetCommonEmmySharp()
        {
            return EmmySharpBuilder.ForThisAssembly()
                .AppendFunction(typeof(LuaRunner).GetMethod("XY"))
                .AppendFunction(typeof(LuaRunner).GetMethod("XYZ"))
                .AppendFunction(typeof(LuaRunner).GetMethod("HSVA"))
                .AppendFunction(typeof(LuaRunner).GetMethod("RGBA"))
                .AppendFunction(typeof(LuaRunner).GetMethod("Log"))
                .AppendFunction(typeof(LuaRunner).GetMethod("ToNumber"))
                .AppendFunction(typeof(LuaRunner).GetMethod("ToBool"));
        }
    }
}