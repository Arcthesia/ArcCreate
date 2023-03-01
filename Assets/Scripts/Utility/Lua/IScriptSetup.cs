using MoonSharp.Interpreter;

namespace ArcCreate.Utilities.Lua
{
    /// <summary>
    /// Interface for setting up a lua script object.
    /// </summary>
    public interface IScriptSetup
    {
        /// <summary>
        /// Set up the lua script.
        /// </summary>
        /// <param name="scriptObject">The script to setup.</param>
        void SetupScript(Script scriptObject);
    }
}