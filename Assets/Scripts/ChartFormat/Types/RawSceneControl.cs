using System.Collections.Generic;

namespace Arc.ChartFormat
{
    public class RawSceneControl : RawEvent
    {
        public string SceneControlTypeName { get; set; }

        public List<object> Arguments { get; set; }
    }
}