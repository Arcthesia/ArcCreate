using System.Collections.Generic;

namespace Arc.ChartFormat
{
    public class RawTimingGroup
    {
        public RawTimingGroup()
        {
            NoInput = false;
            NoClip = false;
            FadingHolds = false;
            AngleX = 0;
            AngleY = 0;
            Side = SideOverride.None;
        }

        public RawTimingGroup(string def, int line = 0)
        {
            if (def == "")
            {
                return;
            }

            string[] split = def.Split(',');
            foreach (string optRaw in split)
            {
                string opt = optRaw.ToLower();
                if (opt.Contains("="))
                {
                    string[] tokens = opt.Split('=');
                    string type = tokens[0];
                    string value = tokens[1];

                    bool valid;
                    float val;
                    switch (type)
                    {
                        case "anglex":
                            valid = Evaluator.TryFloat(value, out val);
                            AngleX = valid ? val : 0;
                            break;
                        case "angley":
                            valid = Evaluator.TryFloat(value, out val);
                            AngleY = valid ? val : 0;
                            break;
                        default:
                            throw new ChartFormatException(
                                RawEventType.TimingGroup,
                                opt,
                                File,
                                line,
                                I.S("Format.Exception.TimingGroupPropertiesInvalid"));
                    }
                }
                else
                {
                    switch (opt)
                    {
                        case "noinput":
                            NoInput = true;
                            break;
                        case "noclip":
                            NoClip = true;
                            break;
                        case "light":
                            Side = SideOverride.Light;
                            break;
                        case "conflict":
                            Side = SideOverride.Conflict;
                            break;
                        case "fadingholds":
                            FadingHolds = true;
                            break;
                        default:
                            throw new ChartFormatException(
                                RawEventType.TimingGroup,
                                opt,
                                File,
                                line,
                                I.S("Format.Exception.TimingGroupProperties.Invalid"));
                    }
                }
            }
        }

        public bool NoInput { get; set; } = false;

        public bool NoClip { get; set; } = false;

        public bool FadingHolds { get; set; } = false;

        public float AngleX { get; set; } = 0;

        public float AngleY { get; set; } = 0;

        public SideOverride Side { get; set; }

        public string File { get; set; } = "";

        public bool Editable { get; set; } = true;

        public override string ToString()
        {
            List<string> opts = new List<string>();
            if (NoInput)
            {
                opts.Add("noinput");
            }

            if (NoClip)
            {
                opts.Add("noclip");
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

            if (Side != SideOverride.None)
            {
                opts.Add(Side == SideOverride.Light ? "light" : "conflict");
            }

            return string.Join(",", opts);
        }
    }
}