using System.Collections.Generic;

namespace ArcCreate.ChartFormat
{
    public class RawSceneControl : RawEvent
    {
        public string SceneControlTypeName { get; set; }

        public List<object> Arguments { get; set; }
    }
}