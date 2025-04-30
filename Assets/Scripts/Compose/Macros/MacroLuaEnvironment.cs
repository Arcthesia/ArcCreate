using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using ArcCreate.Utility.InfiniteScroll;
using ArcCreate.Utility.Lua;
using Cysharp.Threading.Tasks;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Compose.Macros
{
    public class MacroLuaEnvironment : IScriptSetup
    {
        private readonly Dictionary<string, MacroDefinition> macros = new Dictionary<string, MacroDefinition>();
        private readonly List<MacroDefinition> macrosTree = new List<MacroDefinition>();
        private readonly MacroPicker picker;
        private readonly Pool<Cell> macroCellPool;
        private readonly float macroCellSize;
        private readonly Persistent persistent = new Persistent();

        private IRequest currentRequest;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private bool blockReload;

        public MacroLuaEnvironment(MacroPicker picker, Pool<Cell> macroCellPool, float macroCellSize)
        {
            this.picker = picker;
            this.macroCellPool = macroCellPool;
            this.macroCellSize = macroCellSize;
        }

        public string MacroDefFolder =>
#if UNITY_STANDALONE_WIN
            new DirectoryInfo(Application.dataPath).Parent.FullName + "/Macros";
#else
            new DirectoryInfo(Application.persistentDataPath).FullName + "/Macros";
#endif

        public string PreBundledMacroDefFolder =>
            new DirectoryInfo(Application.streamingAssetsPath) + "/Macros";

        public void WaitForRequest(IRequest request)
        {
            currentRequest = request;
        }

        public void CancelMacro()
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        public void ExecuteScript(string path, bool includePreBundledPath)
        {
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                Script script = new Script();
                UserData.RegisterAssembly();
                if (includePreBundledPath)
                {
                    LuaRunner.RunScript(File.ReadAllText(path), this, new ScriptLoader(
                        Path.GetDirectoryName(path),
                        PreBundledMacroDefFolder));
                }
                else
                {
                    LuaRunner.RunScript(File.ReadAllText(path), this, new ScriptLoader(Path.GetDirectoryName(path)));
                }
            }
            catch (Exception e)
            {
                string file = Path.GetFileName(path);
                throw new Exception($"Error executing lua script {file}: \n{e.Message}\n{e.StackTrace}");
            }
        }

        public void GenerateEmmyLua()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(MacroService));
            LuaRunner.GetCommonEmmySharp()
                .AppendGroup(assembly, "Macros")
                .AppendFunction(typeof(MacroLuaEnvironment).GetMethod("RemoveMacro"))
                .AppendFunction(typeof(MacroLuaEnvironment).GetMethod("Notify"))
                .AppendFunction(typeof(MacroLuaEnvironment).GetMethod("NotifyWarn"))
                .AppendFunction(typeof(MacroLuaEnvironment).GetMethod("NotifyError"))
                .Build(MacroDefFolder);
        }

        public void SetupScript(Script script)
        {
            script.Globals["addMacro"] = (Action<string, string, string, DynValue>)((parent, id, displayName, macroDef) => AddNode(parent, id, displayName, null, macroDef, script));
            script.Globals["addMacroWithIcon"] = (Action<string, string, string, string, DynValue>)((parent, id, displayName, icon, macroDef) => AddNode(parent, id, displayName, icon, macroDef, script));
            script.Globals["addFolder"] = (Action<string, string, string>)((parent, id, displayName) => AddNode(parent, id, displayName, null, null, script));
            script.Globals["addFolderWithIcon"] = (Action<string, string, string, string>)((parent, id, icon, displayName) => AddNode(parent, id, displayName, icon, null, script));
            script.Globals["removeMacro"] = (Action<string>)RemoveMacro;
            script.Globals["Context"] = new MacroContext();
            script.Globals["Event"] = new Event();
            script.Globals["Macro"] = new Macro(this, script);
            script.Globals["Folder"] = new Folder(this, script);
            script.Globals["DialogField"] = new DialogField();
            script.Globals["DialogInput"] = new DialogInput();
            script.Globals["TrackInput"] = new TrackInput();
            script.Globals["Command"] = new Command();
            script.Globals["EventSelectionInput"] = new EventSelectionInput();
            script.Globals["EventSelectionConstraint"] = new EventSelectionConstraint();
            script.Globals["FieldConstraint"] = new FieldConstraint();
            script.Globals["Persistent"] = persistent;
            script.Globals["notify"] = (Action<object>)Notify;
            script.Globals["notifyWarn"] = (Action<object>)NotifyWarn;
            script.Globals["notifyError"] = (Action<object>)NotifyError;
        }

        public bool TryGetMacro(string macroId, out MacroDefinition macro)
        {
            return macros.TryGetValue(macroId, out macro);
        }

        public async UniTask RunMacro(MacroDefinition macro)
        {
            CancelMacro();
            await UniTask.NextFrame();

            CancellationToken ct = cts.Token;
            picker.Selected = macro;

            try
            {
                DynValue coroutine = macro.Script.CreateCoroutine(macro.Callback);
                coroutine.Coroutine.Resume();

                while (true)
                {
                    while (coroutine.Coroutine.State == CoroutineState.Running)
                    {
                        await UniTask.NextFrame(cancellationToken: ct);
                    }

                    if (coroutine.Coroutine.State == CoroutineState.Suspended)
                    {
                        if (currentRequest == null)
                        {
                            throw new Exception($"Macro yielded but no requests were made. Execution has been halted.");
                        }

                        while (!currentRequest.Complete)
                        {
                            await UniTask.NextFrame(cancellationToken: ct);
                        }

                        coroutine.Coroutine.Resume();
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Services.Popups.Notify(Popups.Severity.Warning, I18n.S("Compose.Notify.Macros.Cancelled"));
            }
            catch (Exception e)
            {
                throw new Exception($"Error executing macro {macro.Id}: \n{e.Message}\n{e.StackTrace}");
            }
            finally
            {
                picker.Selected = null;
            }
        }

        public void ReloadMacros()
        {
            macros.Clear();
            macrosTree.Clear();
            blockReload = true;

            LoadMacrosFrom(MacroDefFolder, true);
            LoadMacrosFrom(PreBundledMacroDefFolder, false);
            UpdateMacroTree();
        }

        [EmmyDoc("Remove a macro or folder. Does nothing if the macro has not been added.")]
        public void RemoveMacro(string id)
        {
            if (!macros.ContainsKey(id))
            {
                return;
            }

            foreach (var node in macrosTree)
            {
                node.RemoveNode(id);
            }

            macros.Remove(id);
            UpdateMacroTree();
        }

        public void Notify(object content)
            => Services.Popups.Notify(Popups.Severity.Info, content.ToString());

        public void NotifyWarn(object content)
            => Services.Popups.Notify(Popups.Severity.Warning, content.ToString());

        public void NotifyError(object content)
            => Services.Popups.Notify(Popups.Severity.Error, content.ToString());

        private void LoadMacrosFrom(string dir, bool includePreBundledPath)
        {
            if (!Directory.Exists(dir))
            {
                return;
            }

            foreach (string path in Directory.GetFiles(dir))
            {
                if (!path.EndsWith(".lua"))
                {
                    continue;
                }

                try
                {
                    ExecuteScript(path, includePreBundledPath);
                }
                catch (Exception e)
                {
                    Debug.LogError(I18n.S("Compose.Exception.LuaScript", new Dictionary<string, object>()
                    {
                        { "Path", path },
                        { "Message", e.Message },
                        { "StackTrace", e.StackTrace },
                    }));
                }
            }

            blockReload = false;
        }

        private void UpdateMacroTree()
        {
            if (blockReload)
            {
                return;
            }

            picker.SetData(macrosTree);
        }

        public void AddNode(string parent, string id, string displayName, string icon, DynValue macroDef, Script script)
        {
            MacroDefinition def = new MacroDefinition(id)
            {
                Pool = macroCellPool,
                Size = macroCellSize,
                Children = new List<CellData>(),
                Icon = icon,
                Name = displayName,
                Script = script,
                Callback = macroDef,
            };

            if (parent != null)
            {
                if (!macros.ContainsKey(parent))
                {
                    AddNode(parent, "Unnamed", "Unnamed", null, null, script);
                }

                MacroDefinition parentMacro = macros[parent];
                parentMacro.Children.Add(def);
            }
            else
            {
                macrosTree.Add(def);
            }

            if (macros.ContainsKey(id))
            {
                macros[id] = def;
            }
            else
            {
                macros.Add(id, def);
            }

            UpdateMacroTree();
        }

        internal void SavePersistent()
        {
            persistent.Dispose();
        }
    }
}