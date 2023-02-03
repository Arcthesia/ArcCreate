using System;

namespace ArcCreate.Compose.Lua
{
    [Serializable]
    internal class InstructionLimitReachedException : ComposeException
    {
        public InstructionLimitReachedException(int limit)
            : base(I18n.S("Compose.Exception.InstructionLimit", limit))
        {
        }
    }
}