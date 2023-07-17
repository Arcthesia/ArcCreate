using System.Collections.Generic;
using ArcCreate.Utility.Parser;

namespace ArcCreate.ChartFormat
{
    public class RawTimingGroup
    {
        // Might be problematic when multiple chart formats is introduced, as this class only serialize / deserialize to aff.
        // Don't wnat to think about it now though.
        public RawTimingGroup()
        {
            NoInput = false;
            NoClip = false;
            FadingHolds = false;
            NoHeightIndicator = false;
            NoShadow = false;
            NoArcCap = false;
            NoHead = false;
            AngleX = 0;
            AngleY = 0;
            ArcResolution = 1;
            Side = SideOverride.None;
        }

        public string Name { get; set; } = null;

        public bool NoInput { get; set; } = false;

        public bool NoClip { get; set; } = false;

        public bool NoHeightIndicator { get; set; } = false;

        public bool NoShadow { get; set; } = false;

        public bool NoHead { get; set; } = false;

        public bool NoArcCap { get; set; } = false;

        public bool FadingHolds { get; set; } = false;

        public float ArcResolution { get; set; } = 1;

        public float AngleX { get; set; } = 0;

        public float AngleY { get; set; } = 0;

        public SideOverride Side { get; set; }

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
                        case "arcresolution":
                            valid = Evaluator.TryFloat(value, out val);
                            val = UnityEngine.Mathf.Clamp(val, 0, 10);
                            tg.ArcResolution = valid ? val : 1;
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
                        case "light":
                            tg.Side = SideOverride.Light;
                            break;
                        case "conflict":
                            tg.Side = SideOverride.Conflict;
                            break;
                        case "fadingholds":
                            tg.FadingHolds = true;
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

            if (Side != SideOverride.None)
            {
                opts.Add(Side == SideOverride.Light ? "light" : "conflict");
            }

            return opts;
        }
    }
}