using ArcCreate.ChartFormat;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyGroup("Macros")]
    public class LuaTimingGroup
    {
        public LuaTimingGroup()
        {
        }

        public LuaTimingGroup(int groupNumber, RawTimingGroup raw)
        {
            Side = "none";
            switch (raw.Side)
            {
                case SideOverride.None:
                    Side = "none";
                    break;
                case SideOverride.Light:
                    Side = "light";
                    break;
                case SideOverride.Conflict:
                    Side = "conflict";
                    break;
            }

            Num = groupNumber;
            Name = raw.Name;
            NoInput = raw.NoInput;
            NoClip = raw.NoClip;
            NoHeightIndicator = raw.NoHeightIndicator;
            NoShadow = raw.NoShadow;
            NoHead = raw.NoHead;
            NoArcCap = raw.NoArcCap;
            FadingHolds = raw.FadingHolds;
            ArcResolution = raw.ArcResolution;
            AngleX = raw.AngleX;
            AngleY = raw.AngleY;
            File = raw.File;
        }

        public int Num { get; set; }

        public string Name { get; set; }

        public bool NoInput { get; set; }

        public bool NoClip { get; set; }

        public bool NoHeightIndicator { get; set; }

        public bool NoShadow { get; set; }

        public bool NoHead { get; set; }

        public bool NoArcCap { get; set; }

        public bool FadingHolds { get; set; }

        public float ArcResolution { get; set; }

        public float AngleX { get; set; }

        public float AngleY { get; set; }

        public string Side { get; set; }

        public string File { get; set; }

        [MoonSharpHidden]
        public RawTimingGroup ToProperty()
        {
            SideOverride s;
            switch (Side.ToLower())
            {
                case "none":
                    s = SideOverride.None;
                    break;
                case "light":
                    s = SideOverride.Light;
                    break;
                case "conflict":
                    s = SideOverride.Conflict;
                    break;
                default:
                    s = SideOverride.None;
                    break;
            }

            return new RawTimingGroup
            {
                Name = Name,
                NoInput = NoInput,
                NoClip = NoClip,
                NoHeightIndicator = NoHeightIndicator,
                NoShadow = NoShadow,
                NoHead = NoHead,
                NoArcCap = NoArcCap,
                FadingHolds = FadingHolds,
                ArcResolution = ArcResolution,
                AngleX = AngleX,
                AngleY = AngleY,
                Side = s,
                File = File,
            };
        }

        public override string ToString()
        {
            return ToProperty().ToString();
        }
    }
}