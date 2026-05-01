using System.Collections.Generic;
using System.Linq;

namespace ArcCreate.ChartFormat
{
    public class RawTimingGroup
    {
        // Might be problematic when multiple chart formats is introduced, as this class only serialize / deserialize to aff.
        // Don't wnat to think about it now though.
        public string Name { get; set; } = null;

        public bool NoInput { get; set; } = false;

        public bool NoClip { get; set; } = false;

        public bool NoHeightIndicator { get; set; } = false;

        public bool NoShadow { get; set; } = false;

        public bool NoHead { get; set; } = false;

        public bool NoArcCap { get; set; } = false;

        public bool NoConnection { get; set; } = false;

        public bool FadingHolds { get; set; } = false;

        public bool IgnoreMirror { get; set; } = false;

        public bool Autoplay { get; set; } = false;

        public Dictionary<JudgementMap, JudgementMap> JudgementMaps { get; set; }
            = new Dictionary<JudgementMap, JudgementMap>();

        public float ArcResolution { get; set; } = 1;

        public float DropRate { get; set; } = 0;

        public float AngleX { get; set; } = 0;

        public float AngleY { get; set; } = 0;

        public float JudgementSizeX { get; set; } = 1;

        public float JudgementSizeY { get; set; } = 1;

        public float JudgementOffsetX { get; set; } = 0;

        public float JudgementOffsetY { get; set; } = 0;

        public float JudgementOffsetZ { get; set; } = 0;

        public SideOverride Side { get; set; } = SideOverride.None;

        public string File { get; set; } = "";

        public bool Editable { get; set; } = true;

        /// <summary>
        /// Parse properties string with a <see cref="ChartReader"/> instance
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="propertiesRaw"></param>
        /// <returns></returns>
        public static RawTimingGroup ParseProperties(ChartReader reader, string propertiesRaw)
        {
            var raw = $"timinggroup({propertiesRaw})" + "{};";
            var tgEvt = ChartReader.ParseEvents(raw).Events.ToList()[0];
            
            return reader.ParseTimingGroupProperties(raw, tgEvt);
        }
        
        private static bool TryGetJudgement(string mapTo, out JudgementMap result)
        {
            switch (mapTo)
            {
                case "max": result = JudgementMap.Max; return true;
                case "perfectearly": result = JudgementMap.PerfectEarly; return true;
                case "goodearly": result = JudgementMap.GoodEarly; return true;
                case "missearly": result = JudgementMap.MissEarly; return true;
                case "perfectlate": result = JudgementMap.PerfectLate; return true;
                case "goodlate": result = JudgementMap.GoodLate; return true;
                case "misslate": result = JudgementMap.MissLate; return true;
                case "perfect": result = JudgementMap.PerfectMapped; return true;
                case "good": result = JudgementMap.GoodMapped; return true;
                case "miss": result = JudgementMap.MissMapped; return true;
                default: result = default; return false;
            }
        }

        public void AddRemapRules(string value, params JudgementMap[] fromJudgements)
        {
            string mapTo = value.Trim('"').ToLower();
            if (TryGetJudgement(mapTo, out JudgementMap res))
            {
                foreach (var j in fromJudgements)
                {
                    JudgementMaps.Add(j, res);
                }
            }
        }

        public string SerializeJudgementMap(JudgementMap j)
        {
            switch (j)
            {
                case JudgementMap.MissEarly: return "missearly";
                case JudgementMap.GoodEarly: return "goodearly";
                case JudgementMap.PerfectEarly: return "perfectearly";
                case JudgementMap.Max: return "max";
                case JudgementMap.PerfectLate: return "perfectlate";
                case JudgementMap.GoodLate: return "goodlate";
                case JudgementMap.MissLate: return "misslate";
                case JudgementMap.PerfectMapped: return "perfect";
                case JudgementMap.GoodMapped: return "good";
                case JudgementMap.MissMapped: return "miss";
                default: return j.ToString().ToLower();
            }
        }
    }
}