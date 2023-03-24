using System.IO;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace ArcCreate.Utility.Lua
{
    public class ScriptLoader : FileSystemScriptLoader
    {
        private readonly string path;

        public ScriptLoader(string path)
        {
            this.path = Path.GetFullPath(path);
            ModulePaths = new string[] { "?", "?.lua" };
        }

        public override bool ScriptFileExists(string name)
        {
            string fullPath = Path.GetFullPath(Path.Combine(path, name));
            if (fullPath.Length < path.Length)
            {
                return false;
            }

            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] != fullPath[i])
                {
                    return false;
                }
            }

            return base.ScriptFileExists(fullPath);
        }

        public override object LoadFile(string name, Table context)
        {
            return base.LoadFile(Path.Combine(path, name), context);
        }
    }
}