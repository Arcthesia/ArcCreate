using System.Collections.Generic;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyDoc("Containment of data retuend from user input requests.")]
    [EmmyGroup("Macros")]
    public class MacroRequest : IRequest
    {
        public Dictionary<string, DynValue> Result { get; set; } = new Dictionary<string, DynValue>();

        [MoonSharpHidden]
        public bool Complete { get; set; }
    }
}