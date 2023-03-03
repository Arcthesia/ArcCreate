using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArcCreate.Gameplay.Data;
using ArcCreate.Gameplay.Scenecontrol;
using ArcCreate.Utilities.Lua;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Compose.EventsEditor
{
    public class ScenecontrolLuaEnvironment : IScriptSetup
    {
        public const int InstructionLimit = int.MaxValue;
        private readonly ScenecontrolTable scTable;
        private readonly Dictionary<string, IScenecontrolType> scenecontrolTypes = new Dictionary<string, IScenecontrolType>();

        public ScenecontrolLuaEnvironment(ScenecontrolTable scTable)
        {
            this.scTable = scTable;
        }

        public void SetupScript(Script script)
        {
            script.Globals["addScenecontrol"] = (Action<string, DynValue, DynValue>)AddScenecontrolType;
            script.Globals["Channel"] = new ValueChannelBuilder();
            script.Globals["StringChannel"] = new StringChannelBuilder();
            script.Globals["TextChannel"] = new TextChannelBuilder();
            script.Globals["Trigger"] = new TriggerBuilder();
            script.Globals["TriggerChannel"] = new TriggerChannelBuilder();
            script.Globals["Scene"] = Services.Gameplay.Scenecontrol.Scene;
            script.Globals["Context"] = new Context();

            // script.Globals["PostProcessing"] = Services.Gameplay.Scenecontrol.PostProcessing;
            script.Globals["notify"] = (Action<object>)((value) => Services.Popups.Notify(Popups.Severity.Info, value.ToString()));
            script.Globals["notifyWarn"] = (Action<object>)((value) => Services.Popups.Notify(Popups.Severity.Warning, value.ToString()));
            script.Globals["notifyError"] = (Action<object>)((value) => Services.Popups.Notify(Popups.Severity.Error, value.ToString()));

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

        public void Rebuild()
        {
            Services.Gameplay.Scenecontrol.ScenecontrolFolder =
                Path.GetDirectoryName(Services.Project.CurrentProject.Path);
            Clean();
            RunScript();
            ExecuteEvents();
        }

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

        private void RunScript()
        {
            Script script = new Script();
            string folderPath = Services.Gameplay.Scenecontrol.ScenecontrolFolder;

            UserData.RegisterAssembly();

            string filePath = Path.Combine(folderPath, "init.lua");
            AddBuiltInTypes();
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                script.DoString(File.ReadAllText(filePath));
                LuaRunner.RunScript(File.ReadAllText(filePath), this, InstructionLimit, new ScriptLoader(folderPath));
            }
            catch (Exception e)
            {
                Clean();
                Debug.LogError(I18n.S("Compose.Exception.LuaScript", new Dictionary<string, object>()
                {
                    { "Path", filePath },
                    { "Message", e.Message },
                    { "StackTrace", e.StackTrace },
                }));
            }
        }

        private void Clean()
        {
            scenecontrolTypes.Clear();
            scTable.ClearTypes();
            Services.Gameplay.Scenecontrol.Clean();
        }

        private void AddBuiltInTypes()
        {
            ScDataSource.SetupChannels();
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

        private void AddScenecontrolType(string name, DynValue argNames, DynValue scDef)
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
    }
}