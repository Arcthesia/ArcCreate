using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ArcCreate.ChartFormat
{
    /// <summary>
    /// Class for writing a single .aff chart file.
    /// </summary>
    public class AffChartFileWriter : IChartFileWriter
    {
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

            this.stream.Flush();
            this.stream.Close();
        }

        private void WriteChartSettings(int audioOffset, float density)
        {
            stream.WriteLine($"AudioOffset:{audioOffset}");
            if (!Mathf.Approximately(density, 1))
            {
                stream.WriteLine($"TimingPointDensityFactor:{density:f1}");
            }

            stream.WriteLine("-");
        }

        private void WriteEvent(RawEvent affEvent, bool doesIndent = false)
        {
            string indent = doesIndent ? "  " : "";
            switch (affEvent.Type)
            {
                case RawEventType.Timing:
                    RawTiming timing = affEvent as RawTiming;
                    stream.WriteLine($"{indent}timing({timing.Timing},{timing.Bpm:f2},{timing.Divisor:f2});");
                    break;

                case RawEventType.Tap:
                    RawTap tap = affEvent as RawTap;
                    stream.WriteLine($"{indent}({tap.Timing},{tap.Lane});");
                    break;

                case RawEventType.Hold:
                    RawHold hold = affEvent as RawHold;
                    stream.WriteLine($"{indent}hold({hold.Timing},{hold.EndTiming},{hold.Lane});");
                    break;

                case RawEventType.Arc:
                    RawArc arc = affEvent as RawArc;
                    string arcStr =
                      $"{indent}arc({arc.Timing},{arc.EndTiming},{arc.XStart:f2},{arc.XEnd:f2},"
                    + $"{arc.LineType},{arc.YStart:f2},{arc.YEnd:f2},"
                    + $"{arc.Color},{arc.Sfx ?? "none"},{(arc.IsTrace ? "true" : "false")})";

                    if (arc.ArcTaps != null && arc.ArcTaps.Count != 0)
                    {
                        arcStr += "[";
                        for (int i = 0; i < arc.ArcTaps.Count; ++i)
                        {
                            arcStr += $"arctap({arc.ArcTaps[i]})";
                            if (i != arc.ArcTaps.Count - 1)
                            {
                                arcStr += ",";
                            }
                        }

                        arcStr += "]";
                    }

                    arcStr += ";";
                    stream.WriteLine(arcStr);
                    break;

                case RawEventType.Camera:
                    RawCamera cam = affEvent as RawCamera;
                    string camStr =
                      $"{indent}camera({cam.Timing},{cam.Move.x:f2},{cam.Move.y:f2},{cam.Move.z:f2},"
                    + $"{cam.Rotate.x:f2},{cam.Rotate.y:f2},{cam.Rotate.z:f2},"
                    + $"{cam.CameraType},{cam.Duration});";
                    stream.WriteLine(camStr);
                    break;

                case RawEventType.SceneControl:
                    RawSceneControl scc = affEvent as RawSceneControl;

                    if (scc.Arguments.Count == 0)
                    {
                        stream.WriteLine($"{indent}scenecontrol({scc.Timing},{scc.SceneControlTypeName});");
                    }
                    else
                    {
                        string parameterString = ",";
                        foreach (object parameter in scc.Arguments)
                        {
                            if (parameter is string)
                            {
                                string s = parameter.ToString();
                                s = s.Replace("\"", "\\\"");
                                parameterString += "\"" + s + "\",";
                            }
                            else if (parameter is float f)
                            {
                                parameterString += f.ToString("G") + ",";
                            }
                        }

                        parameterString = parameterString.Remove(parameterString.Length - 1);
                        stream.WriteLine($"{indent}scenecontrol({scc.Timing},{scc.SceneControlTypeName}{parameterString});");
                    }

                    break;
                case RawEventType.Include:
                    RawInclude incl = affEvent as RawInclude;
                    stream.WriteLine($"{indent}include({incl.File});");
                    break;
                case RawEventType.Fragment:
                    RawFragment frag = affEvent as RawFragment;
                    stream.WriteLine($"{indent}fragment({frag.Timing},{frag.File});");
                    break;
            }
        }

        private void WriteTimingGroupStart(RawTimingGroup properties)
        {
            stream.WriteLine("timinggroup(" + properties.ToString() + "){");
        }

        private void WriteTimingGroupEnd()
        {
            stream.WriteLine("};");
        }
    }
}