using System;

namespace ArcCreate.Utilities.Lua
{
    [Serializable]
    public class InstructionLimitReachedException : Exception
    {
        public InstructionLimitReachedException(int limit)
            : base(I18n.S("Compose.Exception.InstructionLimit", limit))
        {
        }
    }
}