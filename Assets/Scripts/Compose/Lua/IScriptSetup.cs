using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Lua
{
    public interface IScriptSetup
    {
        void SetupScript(Script scriptObject);
    }
}