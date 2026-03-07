using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using ArcCreate.Utility.Lua;
using Cysharp.Threading.Tasks;
using Google.MaterialDesign.Icons;
using MoonSharp.VsCodeDebugger;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
    public class ScenecontrolDebugService : MonoBehaviour, IScriptDebugSetup
    {
        [SerializeField] private Toggle autoRebuildToggle;

        [SerializeField] private Button debuggerButton;
        [SerializeField] private MaterialIcon debuggerIcon;
        [SerializeField] private TMP_Text debuggerText;

        [SerializeField] private int debuggerClientDetectInterval = 500;
        [SerializeField] private int debuggerClientDetectCount = 60;

        [SerializeField] private int debuggerServerListenPort = 42020;

        private JObject defaultVsCodeLaunchSettingEntry = null!;

        private MoonSharpVsCodeDebugServer debugServer;
        private DebuggerIndicatorState currentState = DebuggerIndicatorState.Disconnected;

        public MoonSharpVsCodeDebugServer InitDebugServer()
        {
            if (currentState == DebuggerIndicatorState.Disabled) return null;

            debugServer ??= new MoonSharpVsCodeDebugServer(debuggerServerListenPort).Start();

            UpdateDebuggerIndicatorState(DebuggerIndicatorState.Disconnected);
            return debugServer;
        }

        private const BindingFlags BindFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        private static bool HasActiveDebuggerClient(MoonSharpVsCodeDebugServer _debugServer)
        {
            // MoonSharp doesn't expose these required fields
            // so use Reflection to get the information

            var fCurrent = _debugServer.GetType().GetField("m_Current", BindFlags)!;
            var currentDebuggerInstance = fCurrent.GetValue(_debugServer);
            var fClient = currentDebuggerInstance.GetType().GetField("m_Client__", BindFlags)!;

            return fClient.GetValue(currentDebuggerInstance) != null;
        }

        private enum DebuggerIndicatorState
        {
            Preparing,
            Connected,
            Disconnected,
            Disabled
        }

        private void UpdateDebuggerIndicatorState(DebuggerIndicatorState state)
        {
            if (currentState == DebuggerIndicatorState.Disabled) return;

            Color targetColor;

            switch (state)
            {
                case DebuggerIndicatorState.Preparing:
                {
                    targetColor = Color.yellow;

                    debuggerButton.interactable = false;
                    break;
                }
                case DebuggerIndicatorState.Connected:
                {
                    targetColor = Color.green;

                    debuggerButton.interactable = false;
                    autoRebuildToggle.isOn = false;
                    autoRebuildToggle.interactable = false;
                    break;
                }
                case DebuggerIndicatorState.Disconnected:
                {
                    targetColor = Color.white;

                    debuggerButton.interactable = true;
                    autoRebuildToggle.interactable = true;
                    break;
                }
                case DebuggerIndicatorState.Disabled:
                {
                    targetColor = new Color(0.8980392f, 0.2235294f, 0.2235294f);

                    debuggerButton.interactable = false;
                    autoRebuildToggle.interactable = true;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            debuggerText.color = targetColor;
            debuggerIcon.color = targetColor;

            currentState = state;
        }

        public async UniTask<bool> AwaitDebuggerAttach()
        {
            UpdateDebuggerIndicatorState(DebuggerIndicatorState.Preparing);

            // timeout: (debuggerClientDetectInterval) ms * debuggerClientDetectCount
            for (int i = 0; i < debuggerClientDetectCount; i++)
            {
                if (HasActiveDebuggerClient(debugServer))
                {
                    UpdateDebuggerIndicatorState(DebuggerIndicatorState.Connected);
                    return true;
                }

                await UniTask.Delay(debuggerClientDetectInterval, DelayType.Realtime);
            }

            UpdateDebuggerIndicatorState(DebuggerIndicatorState.Disconnected);
            return false;
        }

        public void CleanDebugServer()
        {
            if (debugServer?.Current != null)
            {
                debugServer.Detach(debugServer.Current);
            }

            UpdateDebuggerIndicatorState(DebuggerIndicatorState.Disconnected);
        }

        public void GenerateVsCodeLaunchSettings(string filepath)
        {
            filepath = Path.Combine(filepath, ".vscode");
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }

            filepath = Path.Combine(filepath, "launch.json");


            try
            {
                if (File.Exists(filepath))
                {
                    var json = JObject.Parse(File.ReadAllText(filepath));

                    if (!json.ContainsKey("configurations"))
                    {
                        json["configurations"] = new JArray();
                    }

                    var configurations = json["configurations"] as JArray;

                    bool hasMoonSharpConfig = configurations != null &&
                                              configurations.Any(config =>
                                                  config["type"]?.Value<string>() == "moonsharp-debug");

                    if (!hasMoonSharpConfig)
                    {
                        configurations?.Add(defaultVsCodeLaunchSettingEntry);

                        File.WriteAllText(filepath, json.ToString());
                    }
                }
                else
                {
                    File.WriteAllText(filepath, new JObject
                    {
                        ["version"] = "0.2.0",
                        ["configurations"] = new JArray
                        {
                            defaultVsCodeLaunchSettingEntry
                        }
                    }.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse or update launch.json: {ex.Message}");
            }
        }

        private void Awake()
        {
            defaultVsCodeLaunchSettingEntry = new JObject
            {
                ["name"] = "ArcCreate MoonSharp Attach",
                ["type"] = "moonsharp-debug",
                ["request"] = "attach",
                ["debugServer"] = debuggerServerListenPort
            };
            try
            {
                InitDebugServer();
            }
            catch (SocketException ex)
            {
                // catch SocketException, and disable debugger
                Debug.LogWarning(
                    "Debugger has been initialized on another ArcCreate instance, self-disabled for this");

                UpdateDebuggerIndicatorState(DebuggerIndicatorState.Disabled);
            }
        }
    }
}