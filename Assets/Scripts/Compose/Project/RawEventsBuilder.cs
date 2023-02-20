using System.Collections.Generic;
using System.Linq;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Compose.Project
{
    public class RawEventsBuilder
    {
        public List<(RawTimingGroup groups, IEnumerable<RawEvent> events)> GetEvents()
        {
            List<(RawTimingGroup groups, IEnumerable<RawEvent> events)> list = new List<(RawTimingGroup groups, IEnumerable<RawEvent> events)>();
            foreach (TimingGroup tg in Services.Gameplay.Chart.TimingGroups)
            {
                RawTimingGroup rawprop = tg.GroupProperties.ToRaw();

                List<ArcEvent> events = new List<ArcEvent>();

                events.AddRange(tg.GetEventType<Tap>());
                events.AddRange(tg.GetEventType<Hold>());
                events.AddRange(tg.GetEventType<Arc>());
                events.AddRange(tg.GetEventType<ArcTap>());
                events.AddRange(tg.GetEventType<TimingEvent>());
                events.AddRange(Services.Gameplay.Chart.GetAll<CameraEvent>().Where(cam => cam.TimingGroup == tg.GroupNumber));
                events.AddRange(Services.Gameplay.Chart.GetAll<ScenecontrolEvent>().Where(sc => sc.TimingGroup == tg.GroupNumber));
                events.AddRange(tg.ReferenceEvents);

                events.Sort(
                    (a, b) =>
                    {
                        if (a is LongNote la && b is LongNote lb && a.Timing == b.Timing)
                        {
                            return la.EndTiming.CompareTo(lb.EndTiming);
                        }

                        if (a is CameraEvent ca && b is CameraEvent cb && a.Timing == b.Timing)
                        {
                            return ca.Duration.CompareTo(cb.Duration);
                        }

                        return a.Timing.CompareTo(b.Timing);
                    });

                IEnumerable<RawEvent> rawevents = events.Select<ArcEvent, RawEvent>(
                    (ArcEvent ev) =>
                    {
                        switch (ev)
                        {
                            case TimingEvent timing:
                                return new RawTiming
                                {
                                    Timing = timing.Timing,
                                    TimingGroup = timing.TimingGroup,
                                    Bpm = timing.Bpm,
                                    Divisor = timing.Divisor,
                                };
                            case Tap tap:
                                return new RawTap
                                {
                                    Timing = tap.Timing,
                                    TimingGroup = tap.TimingGroup,
                                    Lane = tap.Lane,
                                };
                            case Hold hold:
                                return new RawHold
                                {
                                    Timing = hold.Timing,
                                    EndTiming = hold.EndTiming,
                                    TimingGroup = hold.TimingGroup,
                                    Lane = hold.Lane,
                                };
                            case Arc arc:
                                return new RawArc
                                {
                                    Timing = arc.Timing,
                                    EndTiming = arc.EndTiming,
                                    TimingGroup = arc.TimingGroup,
                                    Color = arc.Color,
                                    IsTrace = arc.IsTrace,
                                    LineType = arc.LineType.ToLineTypeString(),
                                    XEnd = arc.XEnd,
                                    XStart = arc.XStart,
                                    YEnd = arc.YEnd,
                                    YStart = arc.YStart,
                                    Sfx = arc.Sfx,
                                    ArcTaps = arc.ArcTaps.Select(a => a.Timing).ToList(),
                                };
                            case CameraEvent cam:
                                return new RawCamera
                                {
                                    TimingGroup = cam.TimingGroup,
                                    Timing = cam.Timing,
                                    Move = cam.Move,
                                    Rotate = cam.Rotate,
                                    CameraType = cam.CameraType.ToCameraString(),
                                    Duration = cam.Duration,
                                };
                            case ScenecontrolEvent sc:
                                return new RawSceneControl()
                                {
                                    Timing = sc.Timing,
                                    TimingGroup = sc.TimingGroup,
                                    SceneControlTypeName = sc.Typename,
                                    Arguments = sc.Arguments,
                                };
                            case IncludeEvent incl:
                                return new RawInclude()
                                {
                                    Timing = incl.Timing,
                                    TimingGroup = incl.TimingGroup,
                                    File = incl.File,
                                };
                            case FragmentEvent frag:
                                return new RawFragment()
                                {
                                    Timing = frag.Timing,
                                    TimingGroup = frag.TimingGroup,
                                    File = frag.File,
                                };
                            default:
                                return null;
                        }
                    }).ToList();

                list.Add((rawprop, rawevents));
            }

            return list;
        }
    }
}