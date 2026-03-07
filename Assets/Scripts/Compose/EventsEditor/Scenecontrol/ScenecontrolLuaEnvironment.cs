using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ArcCreate.Compose.Navigation;
using ArcCreate.Compose.Popups;
using ArcCreate.Gameplay.Data;
using ArcCreate.Gameplay.Scenecontrol;
using ArcCreate.Utility.Lua;
using Cysharp.Threading.Tasks;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Compose.EventsEditor
{
#if UNITY_EDITOR
    [EditorScope("TestLua")]
#endif
    public class ScenecontrolLuaEnvironment : IScriptSetup
    {
        private readonly ScenecontrolTable scTable;
        private readonly Dictionary<string, IScenecontrolType> scenecontrolTypes = new Dictionary<string, IScenecontrolType>();

        public ScenecontrolLuaEnvironment(ScenecontrolTable scTable)
        {
            this.scTable = scTable;
            UserData.RegisterAssembly(Assembly.GetAssembly(typeof(ScenecontrolService)));
            LuaArithmetic.SetupForBaseType<ValueChannel>();
        }

#if UNITY_EDITOR
        public ScenecontrolLuaEnvironment()
        {
        }

        [EditorAction("TestReimport", true)]
        public void TestReimport()
        {
            string scJson = Services.Gameplay.Scenecontrol.Export();
            Debug.Log(scJson);
            Services.Gameplay.Scenecontrol.Clean();
            Services.Gameplay.Scenecontrol.Import(scJson);
            Services.Gameplay.Scenecontrol.WaitForSceneLoad();
        }
#endif

        public void SetupScript(Script script)
        {
            script.Globals["Channel"] = new ValueChannelBuilder();
            script.Globals["StringChannel"] = new StringChannelBuilder();
            script.Globals["TextChannel"] = new TextChannelBuilder();
            script.Globals["Trigger"] = new TriggerBuilder();
            script.Globals["TriggerChannel"] = new TriggerChannelBuilder();
            script.Globals["Scene"] = Services.Gameplay.Scenecontrol.Scene;
            script.Globals["Context"] = Services.Gameplay.Scenecontrol.Context;
            script.Globals["PostProcessing"] = Services.Gameplay.Scenecontrol.PostProcessing;

            script.Globals["addScenecontrol"] = (Action<string, DynValue, DynValue>)AddScenecontrol;
            script.Globals["notify"] = (Action<object>)Notify;
            script.Globals["notifyWarn"] = (Action<object>)NotifyWarn;
            script.Globals["notifyError"] = (Action<object>)NotifyError;

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Number, typeof(ValueChannel), dyn =>
            {
                return new ConstantChannel((float)dyn.Number);
            });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.String, typeof(StringChannel), dyn =>
            {
                return StringChannelBuilder.Constant(dyn.String);
            });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.String, typeof(TextChannel), dyn =>
            {
                return TextChannelBuilder.Constant(dyn.String);
            });
        }

        public void Rebuild(bool isDebug = false)
        {
            Services.Gameplay.Scenecontrol.ScenecontrolFolder = Values.ScenecontrolFolder;
            Clean();
            RunScript(isDebug).ContinueWith(ExecuteEvents).Forget();
        }

        public void GenerateEmmyLua()
        {
            Assembly scAssembly = Assembly.GetAssembly(typeof(ScenecontrolService));
            LuaRunner.GetCommonEmmySharp()
                .AppendAssembly(scAssembly)
                .AppendFunction(typeof(ScenecontrolLuaEnvironment).GetMethod("AddScenecontrol"))
                .AppendFunction(typeof(ScenecontrolLuaEnvironment).GetMethod("Notify"))
                .AppendFunction(typeof(ScenecontrolLuaEnvironment).GetMethod("NotifyWarn"))
                .AppendFunction(typeof(ScenecontrolLuaEnvironment).GetMethod("NotifyError"))
                .Build(Path.GetDirectoryName(Services.Project.CurrentProject.Path));
        }

        public void AddScenecontrol(string name, DynValue argNames, DynValue scDef)
        {
            if (scenecontrolTypes.ContainsKey(name))
            {
                throw new Exception($"Can not add two scenecontrols with the same name: {name}");
            }

            string[] args;
            try
            {
                args = argNames.Table.Values.Select(val => val.String).ToArray();
            }
            catch
            {
                int count = (int)Math.Round(argNames.Number);
                List<string> arglist = new List<string>();
                for (int i = 1; i <= count; i++)
                {
                    arglist.Add(i.ToString());
                }

                args = arglist.ToArray();
            }

            scenecontrolTypes.Add(name, new LuaScenecontrolType(scDef, args.Length));
            scTable.SetArgument(name, args);
        }

        public void Notify(object content)
            => Services.Popups.Notify(Popups.Severity.Info, content.ToString());

        public void NotifyWarn(object content)
            => Services.Popups.Notify(Popups.Severity.Warning, content.ToString());

        public void NotifyError(object content)
            => Services.Popups.Notify(Popups.Severity.Error, content.ToString());

        private void ExecuteEvents()
        {
            IEnumerable<ScenecontrolEvent> events = Services.Gameplay.Chart.GetAll<ScenecontrolEvent>();
            string lastTypename = "";
            try
            {
                foreach (ScenecontrolEvent ev in events)
                {
                    if (!scenecontrolTypes.ContainsKey(ev.Typename))
                    {
                        continue;
                    }

                    lastTypename = ev.Typename;
                    scenecontrolTypes[ev.Typename].ExecuteCommand(ev);
                }
            }
            catch (Exception e)
            {
                Clean();
                ShowError(I18n.S("Compose.Exception.Scenecontrol", new Dictionary<string, object>()
                {
                    { "Type", lastTypename },
                    { "Message", e.Message },
                    { "StackTrace", e.StackTrace },
                }));
            }
        }

        private void ShowError(string e)
        {
            Debug.LogError(e);
        }

        private async UniTask RunScript(bool isDebug = false)
        {
            string folderPath = Values.ScenecontrolFolder;

            UserData.RegisterAssembly();
            AddBuiltInTypes();

            const string initFileName = "init.lua";

            string currentChartName = Services.Project.CurrentChart.ChartPath;
            string perChartFileName = Path.GetFileNameWithoutExtension(currentChartName) + ".lua";

            string initPath = Path.Combine(folderPath, initFileName);
            string perChartPath = Path.Combine(folderPath, perChartFileName);
            string lastPath = initPath;

            try
            {
                var debugServer = isDebug ? Services.ScenecontrolDebug.InitDebugServer() : null;

                Script initScript = null;
                Script perChartScript = null;

                if (File.Exists(initPath))
                {
                    lastPath = initPath;
                    initScript = LuaRunner.RunScript(await File.ReadAllTextAsync(initPath),
                        this,
                        new ScriptLoader(folderPath),
                        initFileName,
                        folderPath,
                        debugServer);
                }

                if (File.Exists(perChartPath))
                {
                    lastPath = perChartPath;
                    perChartScript = LuaRunner.RunScript(await File.ReadAllTextAsync(perChartPath),
                        this,
                        new ScriptLoader(folderPath),
                        perChartFileName,
                        folderPath,
                        debugServer);
                }

                if (isDebug && debugServer != null)
                {
                    Debug.Log("Waiting for VsCode debugger to attach");
                    bool isAttached = await Services.ScenecontrolDebug.AwaitDebuggerAttach();
                    if (!isAttached)
                    {
                        Debug.LogWarning("VsCode debugger timeout, continue to run the script");
                    }
                    else
                    {
                        Debug.Log("VsCode debugger attached");

                        // update the canvas before hitting breakpoint
                        Canvas.ForceUpdateCanvases();
                        await UniTask.Delay(500); // a window for changes to take place

                        const string debugEntrypoint = "DEBUG_ENTRYPOINT";

                        if (initScript != null)
                        {
                            if (initScript.Globals[debugEntrypoint] != null)
                            {
                                initScript.Call(initScript.Globals[debugEntrypoint]);
                            }
                            else
                            {
                                Debug.Log($"Unable to find debug entrypoint for '{initFileName}'");
                            }
                        }

                        if (perChartScript != null)
                        {
                            if (perChartScript.Globals[debugEntrypoint] != null)
                            {
                                perChartScript.Call(perChartScript.Globals[debugEntrypoint]);
                            }
                            else
                            {
                                Debug.Log($"Unable to find debug entrypoint for '{perChartFileName}'");
                            }
                        }
                    }

                    Services.ScenecontrolDebug.CleanDebugServer();
                    Debug.Log("VsCode debugger detached");
                }
            }
            catch (Exception e)
            {
                Clean();
                Debug.LogError(I18n.S("Compose.Exception.LuaScript", new Dictionary<string, object>()
                {
                    { "Path", lastPath },
                    { "Message", e.Message },
                    { "StackTrace", e.StackTrace },
                }));
            }

            Services.Gameplay.Scenecontrol.WaitForSceneLoad();
        }

        private void Clean()
        {
            scenecontrolTypes.Clear();
            scTable.ClearTypes();
            Services.Gameplay.Scenecontrol.Clean();
            Services.ScenecontrolDebug.CleanDebugServer();
        }

        private void AddBuiltInTypes()
        {
            AddType(new TrackDisplayType());
            AddType(new HideGroupType());
            AddType(new GroupAlphaType());
            AddType(new EnwidenLanesType());
            AddType(new EnwidenCameraType());
        }

        private void AddType(IBuiltInScenecontrolType type)
        {
            scenecontrolTypes.Add(type.Typename, type);
            scTable.SetArgument(type.Typename, type.ArgumentNames);
        }
    }
}