using ArcCreate.Gameplay.Data;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.EventsEditor
{
    public class LuaScenecontrolType : IScenecontrolType
    {
        private readonly DynValue func;

        public LuaScenecontrolType(DynValue func)
        {
            this.func = func;
        }

        public void ExecuteCommand(ScenecontrolEvent ev)
        {
            func.Function.Call(new LuaScenecontrol
            {
                Timing = ev.Timing,
                TimingGroup = ev.TimingGroup,
                Args = ev.Arguments.ToArray(),
                Type = ev.Typename,
            });
        }
    }
}