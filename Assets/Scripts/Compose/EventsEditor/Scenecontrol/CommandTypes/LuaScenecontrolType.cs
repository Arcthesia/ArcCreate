using ArcCreate.Gameplay.Data;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Compose.EventsEditor
{
    public class LuaScenecontrolType : IScenecontrolType
    {
        private readonly DynValue func;
        private readonly int argCount;

        public LuaScenecontrolType(DynValue func, int argCount)
        {
            this.func = func;
            this.argCount = argCount;
        }

        public void ExecuteCommand(ScenecontrolEvent ev)
        {
            object[] arguments = new object[Mathf.Max(argCount, ev.Arguments.Count)];
            for (int i = 0; i < ev.Arguments.Count; i++)
            {
                arguments[i] = ev.Arguments[i];
            }

            for (int i = ev.Arguments.Count; i < arguments.Length; i++)
            {
                arguments[i] = "";
            }

            func.Function.Call(new LuaScenecontrol
            {
                Timing = ev.Timing,
                TimingGroup = ev.TimingGroup,
                Args = arguments,
                Type = ev.Typename,
            });
        }
    }
}