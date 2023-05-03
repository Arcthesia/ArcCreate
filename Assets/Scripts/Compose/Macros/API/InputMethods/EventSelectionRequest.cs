using System.Collections.Generic;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyDoc("Containment of chart events data.")]
    [EmmyGroup("Macros")]
    public class EventSelectionRequest : IRequest
    {
        [EmmyDoc("Access the lists of events separated by event types, each sorted by timing (e.g. result[\"tap\"] contains all tap notes). All event types are: tap, hold, arc, arctap, timing, camera, scenecontrol")]
        public Dictionary<string, List<LuaChartEvent>> Result { get; set; }

        [MoonSharpHidden]
        public bool Complete { get; set; }

        [EmmyDoc("Access the combined list of events sorted by timing.")]
        public List<LuaChartEvent> ResultCombined
        {
            get
            {
                List<LuaChartEvent> res = new List<LuaChartEvent>();
                foreach (List<LuaChartEvent> list in Result.Values)
                {
                    res.AddRange(list);
                }

                res.Sort((a, b) => a.Timing.CompareTo(b.Timing));
                return res;
            }
        }
    }
}