using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;

namespace ArcCreate.Utilities.Lua
{
    public class LimitInstructionDebugger : IDebugger
    {
        private int counter = 0;
        private readonly int limit;
        private readonly List<DynamicExpression> dynamics = new List<DynamicExpression>();

        public LimitInstructionDebugger(int limit)
        {
            this.limit = limit;
        }

        public void SetSourceCode(SourceCode sourceCode)
        {
        }

        public void SetByteCode(string[] byteCode)
        {
        }

        public bool IsPauseRequested()
        {
            return true;
        }

        public bool SignalRuntimeException(ScriptRuntimeException ex)
        {
            return false;
        }

        public DebuggerAction GetAction(int ip, SourceRef sourceref)
        {
            counter += 1;

            if (counter > limit)
            {
                throw new InstructionLimitReachedException(limit);
            }

            return new DebuggerAction()
            {
                Action = DebuggerAction.ActionType.StepIn,
            };
        }

        public void SignalExecutionEnded()
        {
        }

        public void Update(WatchType watchType, IEnumerable<WatchItem> items)
        {
        }

        public List<DynamicExpression> GetWatchItems()
        {
            return dynamics;
        }

        public void RefreshBreakpoints(IEnumerable<SourceRef> refs)
        {
        }

        public DebuggerCaps GetDebuggerCaps()
        {
            return DebuggerCaps.CanDebugSourceCode | DebuggerCaps.HasLineBasedBreakpoints;
        }

        public void SetDebugService(DebugService debugService)
        {
        }
    }
}