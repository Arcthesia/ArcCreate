using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UnityEngine;

namespace ArcCreate.Compose.Lua
{
    public static class LuaRunner
    {
        private static bool hasRegisteredAssembly = false;

        /// <summary>
        /// Runs a string of lua script.
        /// </summary>
        /// <param name="script">The script to run.</param>
        /// <param name="setup">The object to setup the script, for binding methods.</param>
        /// <param name="limitInstructionCountTo">The instruction limit.</param>
        /// <param name="scriptLoader">The script loader instance for loading script from file system.</param>
        public static void RunScript(string script, IScriptSetup setup, int limitInstructionCountTo = int.MaxValue, FileSystemScriptLoader scriptLoader = null)
        {
            Script scriptObject = new Script();

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

            scriptObject.AttachDebugger(new LimitInstructionDebugger(limitInstructionCountTo));
            scriptObject.DoString(script);
        }

        public static void RegisterCommon(Script scriptObject)
        {
            scriptObject.Globals["xy"] = (Func<float, float, XY>)((x, y) => new XY(x, y));
            scriptObject.Globals["xyz"] = (Func<float, float, float, XYZ>)((x, y, z) => new XYZ(x, y, z));
            scriptObject.Globals["hsva"] = (Func<float, float, float, float, HSVA>)((h, s, v, a) => new HSVA(h, s, v, a));
            scriptObject.Globals["rgba"] = (Func<float, float, float, float, RGBA>)((r, g, b, a) => new RGBA(r, g, b, a));
            scriptObject.Globals["rgba"] = (Func<float, float, float, float, RGBA>)((r, g, b, a) => new RGBA(r, g, b, a));
            scriptObject.Globals["Convert"] = new Convert();

            scriptObject.Globals["log"] = (Action<object>)((value) => Debug.Log(value.ToString()));
            scriptObject.Globals["notify"] = (Action<object>)((value) => Services.Popups.Notify(Popups.Severity.Info, value.ToString()));
            scriptObject.Globals["notifyWarning"] = (Action<object>)((value) => Services.Popups.Notify(Popups.Severity.Warning, value.ToString()));
            scriptObject.Globals["notifyError"] = (Action<object>)((value) => Services.Popups.Notify(Popups.Severity.Error, value.ToString()));
            scriptObject.Globals["toNumber"] = (Func<DynValue, double>)((value) =>
                {
                    if (double.TryParse(value.String, out double result))
                    {
                        return result;
                    }

                    return 0;
                });
            scriptObject.Globals["toBool"] = (Func<DynValue, bool>)((value) =>
                {
                    if (bool.TryParse(value.String, out bool result))
                    {
                        return result;
                    }

                    return false;
                });
        }
    }
}