using MoonSharp.Interpreter;

namespace ArcCreate.Compose.EventsEditor
{
    [MoonSharpUserData]
    internal class LuaScenecontrol
    {
        public int Timing { get; set; }

        public int TimingGroup { get; set; }

        public object[] Args { get; set; }

        public string Type { get; set; }
    }
}