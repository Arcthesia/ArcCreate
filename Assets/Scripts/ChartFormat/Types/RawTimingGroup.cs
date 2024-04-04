using System.Collections.Generic;
using ArcCreate.Utility.Parser;

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

        public Dictionary<JudgementMap, JudgementMap> JudgementMaps { get; }
            = new Dictionary<JudgementMap, JudgementMap>();

        public float ArcResolution { get; set; } = 1;

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

        public static Result<RawTimingGroup, ChartError> Parse(string def, int lineNumber = 0)
        {
            var tg = new RawTimingGroup();
            if (def == "")
            {
                return tg;
            }

            def += ",";
            StringParser parser = new StringParser(def);
            while (!parser.HasEnded)
            {
                if (!parser.ReadString(",").TryUnwrap(out TextSpan<string> optRaw, out ParsingError e))
                {
                    return ChartError.Parsing(def, lineNumber, RawEventType.TimingGroup, e);
                }

                string opt = optRaw.Value.Trim().ToLower();
                if (opt.Contains("="))
                {
                    string[] tokens = opt.Split('=');
                    string type = tokens[0];
                    string value = tokens[1];

                    bool valid;
                    float val;
                    switch (type)
                    {
                        case "name":
                            tg.Name = value.Trim('"');
                            break;
                        case "anglex":
                            valid = Evaluator.TryFloat(value, out val);
                            tg.AngleX = valid ? val : 0;
                            break;
                        case "angley":
                            valid = Evaluator.TryFloat(value, out val);
                            tg.AngleY = valid ? val : 0;
                            break;
                        case "judgesizex":
                            valid = Evaluator.TryFloat(value, out val);
                            tg.JudgementSizeX = valid ? val : 1;
                            break;
                        case "judgesizey":
                            valid = Evaluator.TryFloat(value, out val);
                            tg.JudgementSizeY = valid ? val : 1;
                            break;
                        case "judgeoffsetx":
                            valid = Evaluator.TryFloat(value, out val);
                            tg.JudgementOffsetX = valid ? val : 1;
                            break;
                        case "judgeoffsety":
                            valid = Evaluator.TryFloat(value, out val);
                            tg.JudgementOffsetY = valid ? val : 1;
                            break;
                        case "judgeoffsetz":
                            valid = Evaluator.TryFloat(value, out val);
                            tg.JudgementOffsetZ = valid ? val : 1;
                            break;
                        case "arcresolution":
                            valid = Evaluator.TryFloat(value, out val);
                            val = UnityEngine.Mathf.Clamp(val, 0, 10);
                            tg.ArcResolution = valid ? val : 1;
                            break;
                        case "max":
                            AddRemapRules(tg, value, JudgementMap.Max);
                            break;
                        case "perfect":
                            AddRemapRules(tg, value, JudgementMap.PerfectEarly, JudgementMap.PerfectLate);
                            break;
                        case "perfectearly":
                            AddRemapRules(tg, value, JudgementMap.PerfectEarly);
                            break;
                        case "perfectlate":
                            AddRemapRules(tg, value, JudgementMap.PerfectLate);
                            break;
                        case "good":
                            AddRemapRules(tg, value, JudgementMap.GoodEarly, JudgementMap.GoodLate);
                            break;
                        case "goodearly":
                            AddRemapRules(tg, value, JudgementMap.GoodEarly);
                            break;
                        case "goodlate":
                            AddRemapRules(tg, value, JudgementMap.GoodLate);
                            break;
                        case "miss":
                            AddRemapRules(tg, value, JudgementMap.MissEarly, JudgementMap.MissLate);
                            break;
                        case "missearly":
                            AddRemapRules(tg, value, JudgementMap.MissEarly);
                            break;
                        case "misslate":
                            AddRemapRules(tg, value, JudgementMap.MissLate);
                            break;
                        default:
                            return ChartError.Property(
                                def,
                                lineNumber,
                                RawEventType.TimingGroup,
                                optRaw.StartPos,
                                optRaw.Length,
                                ChartError.Kind.TimingGroupPropertiesInvalid);
                    }
                }
                else
                {
                    switch (opt)
                    {
                        case "noinput":
                            tg.NoInput = true;
                            break;
                        case "noclip":
                            tg.NoClip = true;
                            break;
                        case "noheightindicator":
                            tg.NoHeightIndicator = true;
                            break;
                        case "nohead":
                            tg.NoHead = true;
                            break;
                        case "noshadow":
                            tg.NoShadow = true;
                            break;
                        case "noarccap":
                            tg.NoArcCap = true;
                            break;
                        case "noconnection":
                            tg.NoConnection = true;
                            break;
                        case "light":
                            tg.Side = SideOverride.Light;
                            break;
                        case "conflict":
                            tg.Side = SideOverride.Conflict;
                            break;
                        case "fadingholds":
                            tg.FadingHolds = true;
                            break;
                        case "ignoremirror":
                            tg.IgnoreMirror = true;
                            break;
                        case "autoplay":
                            tg.Autoplay = true;
                            break;
                        default:
                            return ChartError.Property(
                                def,
                                lineNumber,
                                RawEventType.TimingGroup,
                                optRaw.StartPos,
                                optRaw.Length,
                                ChartError.Kind.TimingGroupPropertiesInvalid);
                    }
                }
            }

            return tg;
        }

        public override string ToString()
        {
            var opts = GetPropertyStrings(true);
            return string.Join(",", opts);
        }

        public string ToStringWithoutName()
        {
            var opts = GetPropertyStrings(false);
            return string.Join(",", opts);
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

        private static void AddRemapRules(RawTimingGroup tg, string value, params JudgementMap[] fromJudgements)
        {
            string mapTo = value.Trim('"').ToLower();
            if (TryGetJudgement(mapTo, out JudgementMap res))
            {
                foreach (var j in fromJudgements)
                {
                    tg.JudgementMaps.Add(j, res);
                }
            }
        }

        private List<string> GetPropertyStrings(bool withName)
        {
            List<string> opts = new List<string>();
            if (withName && !string.IsNullOrEmpty(Name))
            {
                opts.Add($"name=\"{Name}\"");
            }

            if (NoInput)
            {
                opts.Add("noinput");
            }

            if (NoClip)
            {
                opts.Add("noclip");
            }

            if (NoHeightIndicator)
            {
                opts.Add("noheightindicator");
            }

            if (NoHead)
            {
                opts.Add("nohead");
            }

            if (NoShadow)
            {
                opts.Add("noshadow");
            }

            if (NoArcCap)
            {
                opts.Add("noarccap");
            }

            if (FadingHolds)
            {
                opts.Add("fadingholds");
            }

            if (IgnoreMirror)
            {
                opts.Add("ignoremirror");
            }

            if (Autoplay)
            {
                opts.Add("autoplay");
            }

            if (NoConnection)
            {
                opts.Add("noconnection");
            }

            if (JudgementMaps.TryGetValue(JudgementMap.Max, out JudgementMap maxTo))
            {
                opts.Add($"max={GetStringFrom(maxTo)}");
            }

            if (JudgementMaps.TryGetValue(JudgementMap.PerfectEarly, out JudgementMap pearlyTo)
            && JudgementMaps.TryGetValue(JudgementMap.PerfectLate, out JudgementMap plateTo)
            && pearlyTo == plateTo)
            {
                opts.Add($"perfect={GetStringFrom(pearlyTo)}");
            }
            else if (JudgementMaps.TryGetValue(JudgementMap.PerfectEarly, out JudgementMap pe))
            {
                opts.Add($"perfectearly={GetStringFrom(pe)}");
            }
            else if (JudgementMaps.TryGetValue(JudgementMap.PerfectLate, out JudgementMap pl))
            {
                opts.Add($"perfectlate={GetStringFrom(pl)}");
            }

            if (JudgementMaps.TryGetValue(JudgementMap.GoodEarly, out JudgementMap gearlyTo)
            && JudgementMaps.TryGetValue(JudgementMap.GoodLate, out JudgementMap glateTo)
            && gearlyTo == glateTo)
            {
                opts.Add($"good={GetStringFrom(gearlyTo)}");
            }
            else if (JudgementMaps.TryGetValue(JudgementMap.GoodEarly, out JudgementMap ge))
            {
                opts.Add($"goodearly={GetStringFrom(ge)}");
            }
            else if (JudgementMaps.TryGetValue(JudgementMap.GoodLate, out JudgementMap gl))
            {
                opts.Add($"goodlate={GetStringFrom(gl)}");
            }

            if (JudgementMaps.TryGetValue(JudgementMap.MissEarly, out JudgementMap mearlyTo)
            && JudgementMaps.TryGetValue(JudgementMap.MissLate, out JudgementMap mlateTo)
            && mearlyTo == mlateTo)
            {
                opts.Add($"miss={GetStringFrom(mearlyTo)}");
            }
            else if (JudgementMaps.TryGetValue(JudgementMap.MissEarly, out JudgementMap me))
            {
                opts.Add($"missearly={GetStringFrom(me)}");
            }
            else if (JudgementMaps.TryGetValue(JudgementMap.MissLate, out JudgementMap ml))
            {
                opts.Add($"misslate={GetStringFrom(ml)}");
            }

            if (AngleX != 0)
            {
                opts.Add($"anglex={AngleX:f2}");
            }

            if (AngleY != 0)
            {
                opts.Add($"angley={AngleY:f2}");
            }

            if (ArcResolution != 1)
            {
                opts.Add($"arcresolution={ArcResolution:f1}");
            }

            if (JudgementOffsetX != 0)
            {
                opts.Add($"judgeoffsetx={JudgementOffsetX:f1}");
            }

            if (JudgementOffsetY != 0)
            {
                opts.Add($"judgeoffsety={JudgementOffsetY:f1}");
            }

            if (JudgementOffsetZ != 0)
            {
                opts.Add($"judgeoffsetz={JudgementOffsetZ:f1}");
            }

            if (JudgementSizeX != 1)
            {
                opts.Add($"judgesizex={JudgementSizeX:f1}");
            }

            if (JudgementSizeY != 1)
            {
                opts.Add($"judgesizey={JudgementSizeY:f1}");
            }

            if (Side != SideOverride.None)
            {
                opts.Add(Side == SideOverride.Light ? "light" : "conflict");
            }

            return opts;
        }

        private string GetStringFrom(JudgementMap j)
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
