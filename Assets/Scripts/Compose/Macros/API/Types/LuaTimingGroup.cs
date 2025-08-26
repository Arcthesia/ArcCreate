using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Chart;
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

        public LuaTimingGroup(RawTimingGroup raw)
        {
            SetProperties(raw);
        }

        public int Num => Instance?.GroupNumber ?? -1;

        public string Name { get; set; }

        public bool NoInput { get; set; }

        public bool NoClip { get; set; }

        public bool NoHeightIndicator { get; set; }

        public bool NoShadow { get; set; }

        public bool NoHead { get; set; }

        public bool NoArcCap { get; set; }

        public bool NoConnection { get; set; }

        public bool FadingHolds { get; set; }

        public bool IgnoreMirror { get; set; }

        public bool Autoplay { get; set; }

        public float ArcResolution { get; set; }

        public float DropRate { get; set; }

        public float AngleX { get; set; }

        public float AngleY { get; set; }

        public float JudgementSizeX { get; set; }

        public float JudgementSizeY { get; set; }

        public float JudgementOffsetX { get; set; }

        public float JudgementOffsetY { get; set; }

        public float JudgementOffsetZ { get; set; }

        public string Side { get; set; }

        public string File { get; set; }

        public Dictionary<string, string> JudgementMaps { get; set; }

        [MoonSharpHidden]
        public TimingGroup Instance { get; set; }

        public bool Attached => Instance != null;

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

            var judgementMaps = new Dictionary<JudgementMap, JudgementMap>();
            foreach (var pair in JudgementMaps)
            {
                var to = ParseJudgementMapTo(pair.Value);
                foreach (var from in ParseJudgementMapFrom(pair.Key))
                {
                    if (judgementMaps.ContainsKey(from))
                    {
                        judgementMaps[from] = to;
                    }
                    else
                    {
                        judgementMaps.Add(from, to);
                    }
                }
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
                NoConnection = NoConnection,
                FadingHolds = FadingHolds,
                IgnoreMirror = IgnoreMirror,
                Autoplay = Autoplay,
                ArcResolution = ArcResolution,
                AngleX = AngleX,
                AngleY = AngleY,
                JudgementSizeX = JudgementSizeX,
                JudgementSizeY = JudgementSizeY,
                JudgementOffsetX = JudgementOffsetX,
                JudgementOffsetY = JudgementOffsetY,
                JudgementOffsetZ = JudgementOffsetZ,
                JudgementMaps = judgementMaps,
                Side = s,
                File = File,
            };
        }

        [EmmyDoc("Set the properties of this group using aff timing group property syntax.")]
        public void SetProperties(string props)
        {
            RawTimingGroup prop = RawTimingGroup.Parse(props).UnwrapOrElse(e => throw new System.Exception(e.Message));
            SetProperties(props);
        }

        [MoonSharpHidden]
        public void SetProperties(RawTimingGroup raw)
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

            var judgementMaps = new Dictionary<string, string>();
            foreach (var pair in raw.JudgementMaps)
            {
                var from = JudgementMapToString(pair.Key);
                var to = JudgementMapToString(pair.Value);
                judgementMaps.Add(from, to);
            }

            var prefixes = new string[] {"perfect", "good", "miss"};
            foreach (var prefix in prefixes)
            {
                if (judgementMaps.TryGetValue($"{prefix}early", out var pe)
                && judgementMaps.TryGetValue($"{prefix}late", out var pl)
                && pe == pl)
                {
                    if (judgementMaps.ContainsKey(prefix))
                    {
                        judgementMaps[prefix] = pe;
                    }
                    else
                    {
                        judgementMaps.Add(prefix, pe);
                    }
                }
            }

            Name = raw.Name;
            NoInput = raw.NoInput;
            NoClip = raw.NoClip;
            NoHeightIndicator = raw.NoHeightIndicator;
            NoShadow = raw.NoShadow;
            NoHead = raw.NoHead;
            NoArcCap = raw.NoArcCap;
            NoConnection = raw.NoConnection;
            IgnoreMirror = raw.IgnoreMirror;
            JudgementSizeX = raw.JudgementSizeX;
            JudgementSizeY = raw.JudgementSizeY;
            JudgementOffsetX = raw.JudgementOffsetX;
            JudgementOffsetY = raw.JudgementOffsetY;
            JudgementOffsetZ = raw.JudgementOffsetZ;
            JudgementMaps = judgementMaps;
            FadingHolds = raw.FadingHolds;
            Autoplay = raw.Autoplay;
            ArcResolution = raw.ArcResolution;
            AngleX = raw.AngleX;
            AngleY = raw.AngleY;
            File = raw.File;
        }

        public override string ToString()
        {
            return ToProperty().ToString();
        }

        [EmmyDoc("Create a command that saves changes made to this timing group.")]
        public LuaChartCommand Save()
        {
            if (Instance == null)
            {
                return new LuaChartCommand(addedTG: new List<LuaTimingGroup>() { this });
            }
            else
            {
                return new LuaChartCommand(editedTG: new List<LuaTimingGroup>() { this });
            }
        }

        [EmmyDoc("Create a command that delete current timing group, if it's connected to a real group in the chart.")]
        public LuaChartCommand Delete()
        {
            if (Instance == null && Instance.GroupNumber != 0)
            {
                return Command.Create();
            }

            return new LuaChartCommand(removedTG: new List<LuaTimingGroup>() { this });
        }

        [EmmyDoc("Check if the attached instance equals that of another group")]
        public bool InstanceEquals(LuaTimingGroup group)
        {
            return Instance == group.Instance;
        }

        private JudgementMap[] ParseJudgementMapFrom(string key)
        {
            switch (key.ToLower())
            {
                case "max": return new JudgementMap[] { JudgementMap.Max };
                case "perfect": return new JudgementMap[] { JudgementMap.PerfectEarly, JudgementMap.PerfectLate };
                case "perfectearly": return new JudgementMap[] { JudgementMap.PerfectEarly };
                case "perfectlate": return new JudgementMap[] { JudgementMap.PerfectLate };
                case "good": return new JudgementMap[] { JudgementMap.GoodEarly, JudgementMap.GoodLate };
                case "goodearly": return new JudgementMap[] { JudgementMap.GoodEarly };
                case "goodlate": return new JudgementMap[] { JudgementMap.GoodLate };
                case "miss": return new JudgementMap[] { JudgementMap.MissEarly, JudgementMap.MissLate };
                case "missearly": return new JudgementMap[] { JudgementMap.MissEarly };
                case "misslate": return new JudgementMap[] { JudgementMap.MissLate };
            }

            throw new Exception($"Invalid judgement type ${key}");
        }

        private JudgementMap ParseJudgementMapTo(string key)
        {
            switch (key.ToLower())
            {
                case "max": return JudgementMap.Max;
                case "perfect": return JudgementMap.PerfectMapped;
                case "perfectearly": return JudgementMap.PerfectEarly;
                case "perfectlate": return JudgementMap.PerfectLate;
                case "good": return JudgementMap.GoodMapped;
                case "goodearly": return JudgementMap.GoodEarly;
                case "goodlate": return JudgementMap.GoodLate;
                case "miss": return JudgementMap.MissMapped;
                case "missearly": return JudgementMap.MissEarly;
                case "misslate": return JudgementMap.MissLate;
            }

            throw new Exception($"Invalid judgement type ${key}");
        }

        private string JudgementMapToString(JudgementMap value)
        {
            switch (value)
            {
                case JudgementMap.Max: return "max";
                case JudgementMap.PerfectMapped: return "perfect";
                case JudgementMap.PerfectEarly: return "perfectearly";
                case JudgementMap.PerfectLate: return "perfectlate";
                case JudgementMap.GoodMapped: return "good";
                case JudgementMap.GoodEarly: return "goodearly";
                case JudgementMap.GoodLate: return "goodlate";
                case JudgementMap.MissMapped: return "miss";
                case JudgementMap.MissEarly: return "missearly";
                case JudgementMap.MissLate: return "misslate";
            }

            // Unreachable
            return null;
        }
    }
}