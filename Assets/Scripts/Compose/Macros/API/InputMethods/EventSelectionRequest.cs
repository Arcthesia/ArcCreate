using System;
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
        [MoonSharpHidden]
        public bool Complete { get; set; }

        [EmmyDoc("Access the lists of events separated by event types, each sorted by timing (e.g. result[\"tap\"] contains all tap notes). All event types are: tap, hold, arc, arctap, timing, camera, scenecontrol")]
        public Dictionary<string, List<LuaChartEvent>> Result { get; set; }

        [EmmyDoc("Get returned tap notes")]
        public List<LuaChartEvent> Tap => Result["tap"];

        [EmmyDoc("Get returned hold notes")]
        public List<LuaChartEvent> Hold => Result["hold"];

        [EmmyDoc("Get returned arc notes")]
        public List<LuaChartEvent> Arc => Result["arc"];

        [EmmyDoc("Get returned arctap notes")]
        public List<LuaChartEvent> Arctap => Result["arctap"];

        [EmmyDoc("Get returned timing events")]
        public List<LuaChartEvent> Timing => Result["timing"];

        [EmmyDoc("Get returned scenecontrol events")]
        public List<LuaChartEvent> Scenecontrol => Result["scenecontrol"];

        [EmmyDoc("Get returned camera events")]
        public List<LuaChartEvent> Camera => Result["camera"];

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