using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcCreate.ChartFormat
{
    /// <summary>
    /// Class for writing a single .aff chart file.
    /// </summary>
    public class ArcCreateChartFileWriter : IChartFileWriter
    {
        public static ArcCreateChartFileWriter Instance => new();

        private StreamWriter stream;

        /// <summary>
        /// Write a chart into the provided <see cref="StreamWriter"/> object.
        /// </summary>
        /// <param name="stream">The stream to write into.</param>
        /// <param name="audioOffset">Global AudioOffset setting of the chart.</param>
        /// <param name="density">Global TimingPointDensityFactor setting of the chart.</param>
        /// <param name="groups">List of timing groups,
        /// each being a RawTimingGroup property object, and an IEnumerable of events.</param>
        public void Write(
            StreamWriter stream,
            int audioOffset,
            float density,
            IEnumerable<(RawTimingGroup properties, IEnumerable<RawEvent> events)> groups)
        {
            this.stream = stream;
            StartWritingToStream(audioOffset, density, groups);
            this.stream.Close();
        }

        /// <summary>
        /// Serialize the chart into a string.
        /// </summary>
        /// <param name="audioOffset">Global AudioOffset setting of the chart.</param>
        /// <param name="density">Global TimingPointDensityFactor setting of the chart.</param>
        /// <param name="groups">List of timing groups,
        /// each being a RawTimingGroup property object, and an IEnumerable of events.</param>
        /// <returns>The serialized chart.</returns>
        public string WriteToString(
            int audioOffset,
            float density,
            IEnumerable<(RawTimingGroup properties, IEnumerable<RawEvent> events)> groups)
        {
            var memoryStream = new MemoryStream();
            stream = new StreamWriter(memoryStream);
            StartWritingToStream(audioOffset, density, groups);
            memoryStream.Position = 0;
            string result = new StreamReader(memoryStream).ReadToEnd();
            memoryStream.Close();
            return result;
        }

        private void StartWritingToStream(
            int audioOffset,
            float density,
            IEnumerable<(RawTimingGroup properties, IEnumerable<RawEvent> events)> groups)
        {
            bool baseGroup = true;
            foreach (var (properties, events) in groups)
            {
                if (!baseGroup)
                {
                    WriteTimingGroupStart(properties);
                }
                else
                {
                    WriteChartSettings(audioOffset, density);
                }

                foreach (var e in events)
                {
                    WriteEvent(e, !baseGroup);
                }

                if (!baseGroup)
                {
                    WriteTimingGroupEnd();
                }

                baseGroup = false;
            }

            stream.Flush();
        }

        private void WriteChartSettings(int audioOffset, float density)
        {
            stream.WriteLine($"AudioOffset:{audioOffset}");
            if (!Mathf.Approximately(density, 1))
            {
                stream.WriteLine($"TimingPointDensityFactor:{density:f1}");
            }

            stream.WriteLine("-");
            stream.Flush();
        }

        private void WriteEvent(RawEvent affEvent, bool doesIndent = false)
        {
            string indent = doesIndent ? "  " : "";
            switch (affEvent.Type)
            {
                case RawEventType.Timing:
                    RawTiming timing = affEvent as RawTiming;
                    stream.WriteLine($"{indent}{SerializeTiming(timing)}");
                    break;

                case RawEventType.Tap:
                    RawTap tap = affEvent as RawTap;
                    stream.WriteLine($"{indent}{SerializeTap(tap)}");
                    break;

                case RawEventType.Hold:
                    RawHold hold = affEvent as RawHold;
                    stream.WriteLine($"{indent}{SerializeHold(hold)}");
                    break;

                case RawEventType.Arc:
                    RawArc arc = affEvent as RawArc;
                    stream.WriteLine($"{indent}{SerializeArc(arc)}");
                    break;

                case RawEventType.Camera:
                    RawCamera cam = affEvent as RawCamera;
                    stream.WriteLine($"{indent}{SerializeCamera(cam)}");
                    break;

                case RawEventType.SceneControl:
                    RawSceneControl sc = affEvent as RawSceneControl;
                    stream.WriteLine($"{indent}{SerializeSceneControl(sc)}");
                    break;

                case RawEventType.Include:
                    RawInclude incl = affEvent as RawInclude;
                    stream.WriteLine($"{indent}{SerializeInclude(incl)}");
                    break;

                case RawEventType.Fragment:
                    RawFragment frag = affEvent as RawFragment;
                    stream.WriteLine($"{indent}{SerializeFragment(frag)}");
                    break;
            }

            stream.Flush();
        }

        protected virtual string SerializeTiming(RawTiming timing) =>
            $"timing({timing.Timing},{timing.Bpm:f2},{timing.Divisor:f2});";

        protected virtual string SerializeTap(RawTap tap)
        {
            if (ParsingFormula.IsFloatedLane(tap.Lane))
            {
                return $"({tap.Timing},{tap.Lane:f3});";
            }

            return $"({tap.Timing},{tap.Lane:N0});";
        }


        protected virtual string SerializeHold(RawHold hold)
        {
            if (ParsingFormula.IsFloatedLane(hold.Lane))
            {
                return $"hold({hold.Timing},{hold.EndTiming},{hold.Lane:f3});";
            }

            return $"hold({hold.Timing},{hold.EndTiming},{hold.Lane:N0});";
        }

        protected virtual string SerializeArc(RawArc arc) =>
            "arc(" +
            $"{arc.Timing}," +
            $"{arc.EndTiming}," +
            $"{arc.XStart:f2}," +
            $"{arc.XEnd:f2}," +
            $"{arc.LineType}," +
            $"{arc.YStart:f2}," +
            $"{arc.YEnd:f2}," +
            $"{arc.Color}," +
            $"{arc.Sfx ?? "none"}," +
            (arc.IsTrace ? "true" : "false") +
            ")" +
            SerializeArcTap(arc.ArcTaps) +
            SerializeArcProperties(arc) +
            ";";

        protected virtual string SerializeArcTap(List<RawArcTap> arcTaps)
        {
            if (arcTaps == null || arcTaps.Count == 0) return "";

            var serialized = string.Join(",",
                arcTaps.Select(x => Mathf.Approximately(x.Width, 1)
                    ? $"arctap({x.Timing})"
                    : $"arctap({x.Timing},{x.Width:f2})"));

            return $"[{serialized}]";
        }

        protected virtual string SerializeArcProperties(RawArc arc)
        {
            List<string> serialized = new();

            if (!Mathf.Approximately(arc.ArcResolution, 1))
            {
                serialized.Add($"arcresolution: {arc.ArcResolution:f2}");
            }

            if (arc.TraceColor.HasValue)
            {
                serialized.Add($"tracecolor: #{ColorUtility.ToHtmlStringRGBA(arc.TraceColor.Value)}");
            }

            return serialized.Count > 0 ? $"< {string.Join(", ", serialized)} >" : "";
        }

        protected virtual string SerializeCamera(RawCamera cam) =>
            "camera(" +
            $"{cam.Timing}," +
            $"{cam.Move.x:f2}," +
            $"{cam.Move.y:f2}," +
            $"{cam.Move.z:f2}," +
            $"{cam.Rotate.x:f2}," +
            $"{cam.Rotate.y:f2}," +
            $"{cam.Rotate.z:f2}," +
            $"{cam.CameraType}," +
            $"{cam.Duration});";

        protected virtual string SerializeSceneControl(RawSceneControl sc)
        {
            if (sc.Arguments.Count == 0)
            {
                return $"scenecontrol({sc.Timing},{sc.SceneControlTypeName});";
            }

            var parameters = string.Join(",", sc.Arguments
                .Select(x => x switch
                {
                    string s => s,
                    int i => i.ToString(),
                    float f => f.ToString(CultureInfo.InvariantCulture),
                    _ => null
                })
            );

            return $"scenecontrol({sc.Timing},{sc.SceneControlTypeName},{parameters});";
        }

        protected virtual string SerializeInclude(RawInclude incl) => $"include({incl.File});";

        protected virtual string SerializeFragment(RawFragment frag) => $"include({frag.Timing},{frag.File});";

        public virtual string SerializeTimingGroup(RawTimingGroup properties, bool withName)
        {
            var opts = new List<string>();

            if (withName && !string.IsNullOrEmpty(properties.Name)) opts.Add($"name=\"{properties.Name}\"");

            if (properties.NoInput) opts.Add("noinput");
            if (properties.NoClip) opts.Add("noclip");
            if (properties.NoHeightIndicator) opts.Add("noheightindicator");
            if (properties.NoHead) opts.Add("nohead");
            if (properties.NoShadow) opts.Add("noshadow");
            if (properties.NoArcCap) opts.Add("noarccap");
            if (properties.FadingHolds) opts.Add("fadingholds");
            if (properties.IgnoreMirror) opts.Add("ignoremirror");
            if (properties.Autoplay) opts.Add("autoplay");
            if (properties.NoConnection) opts.Add("noconnection");

            if (properties.JudgementMaps.TryGetValue(JudgementMap.Max, out JudgementMap maxTo))
            {
                opts.Add($"max={properties.SerializeJudgementMap(maxTo)}");
            }

            if (properties.JudgementMaps.TryGetValue(JudgementMap.PerfectEarly, out JudgementMap pearlyTo)
                && properties.JudgementMaps.TryGetValue(JudgementMap.PerfectLate, out JudgementMap plateTo)
                && pearlyTo == plateTo)
            {
                opts.Add($"perfect={properties.SerializeJudgementMap(pearlyTo)}");
            }
            else
            {
                if (properties.JudgementMaps.TryGetValue(JudgementMap.PerfectEarly, out JudgementMap pe))
                {
                    opts.Add($"perfectearly={properties.SerializeJudgementMap(pe)}");
                }

                if (properties.JudgementMaps.TryGetValue(JudgementMap.PerfectLate, out JudgementMap pl))
                {
                    opts.Add($"perfectlate={properties.SerializeJudgementMap(pl)}");
                }
            }

            if (properties.JudgementMaps.TryGetValue(JudgementMap.GoodEarly, out JudgementMap gearlyTo)
                && properties.JudgementMaps.TryGetValue(JudgementMap.GoodLate, out JudgementMap glateTo)
                && gearlyTo == glateTo)
            {
                opts.Add($"good={properties.SerializeJudgementMap(gearlyTo)}");
            }
            else
            {
                if (properties.JudgementMaps.TryGetValue(JudgementMap.GoodEarly, out JudgementMap ge))
                {
                    opts.Add($"goodearly={properties.SerializeJudgementMap(ge)}");
                }

                if (properties.JudgementMaps.TryGetValue(JudgementMap.GoodLate, out JudgementMap gl))
                {
                    opts.Add($"goodlate={properties.SerializeJudgementMap(gl)}");
                }
            }

            if (properties.JudgementMaps.TryGetValue(JudgementMap.MissEarly, out JudgementMap mearlyTo)
                && properties.JudgementMaps.TryGetValue(JudgementMap.MissLate, out JudgementMap mlateTo)
                && mearlyTo == mlateTo)
            {
                opts.Add($"miss={properties.SerializeJudgementMap(mearlyTo)}");
            }
            else
            {
                if (properties.JudgementMaps.TryGetValue(JudgementMap.MissEarly, out JudgementMap me))
                {
                    opts.Add($"missearly={properties.SerializeJudgementMap(me)}");
                }

                if (properties.JudgementMaps.TryGetValue(JudgementMap.MissLate, out JudgementMap ml))
                {
                    opts.Add($"misslate={properties.SerializeJudgementMap(ml)}");
                }
            }

            if (!Mathf.Approximately(properties.AngleX, 0)) opts.Add($"anglex={properties.AngleX:f2}");
            if (!Mathf.Approximately(properties.AngleY, 0)) opts.Add($"angley={properties.AngleY:f2}");

            if (!Mathf.Approximately(properties.ArcResolution, 1))
                opts.Add($"arcresolution={properties.ArcResolution:f1}");

            if (!Mathf.Approximately(properties.DropRate, 0))
                opts.Add($"droprate={properties.DropRate:f2}");

            if (!Mathf.Approximately(properties.JudgementOffsetX, 0))
                opts.Add($"judgeoffsetx={properties.JudgementOffsetX:f1}");

            if (!Mathf.Approximately(properties.JudgementOffsetY, 0))
                opts.Add($"judgeoffsety={properties.JudgementOffsetY:f1}");

            if (!Mathf.Approximately(properties.JudgementOffsetZ, 0))
                opts.Add($"judgeoffsetz={properties.JudgementOffsetZ:f1}");

            if (!Mathf.Approximately(properties.JudgementSizeX, 1))
                opts.Add($"judgesizex={properties.JudgementSizeX:f1}");

            if (!Mathf.Approximately(properties.JudgementSizeY, 1))
                opts.Add($"judgesizey={properties.JudgementSizeY:f1}");

            if (properties.Side != SideOverride.None)
            {
                opts.Add(properties.Side == SideOverride.Light ? "light" : "conflict");
            }

            return string.Join(",", opts);
        }


        private void WriteTimingGroupStart(RawTimingGroup properties)
        {
            stream.WriteLine("timinggroup(" + SerializeTimingGroup(properties, true) + "){");
            stream.Flush();
        }

        private void WriteTimingGroupEnd()
        {
            stream.WriteLine("};");
            stream.Flush();
        }

        private static bool HasDecimal(float value, float epsilon = 0.001f)
        {
            return Mathf.Abs(value % 1) > epsilon;
        }
    }
}