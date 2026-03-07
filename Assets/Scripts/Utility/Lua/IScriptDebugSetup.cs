using Cysharp.Threading.Tasks;
using MoonSharp.VsCodeDebugger;

namespace ArcCreate.Utility.Lua
{
    public interface IScriptDebugSetup
    {
        MoonSharpVsCodeDebugServer InitDebugServer();
        UniTask<bool> AwaitDebuggerAttach();
        void CleanDebugServer();

        void GenerateVsCodeLaunchSettings(string filepath);
    }
}