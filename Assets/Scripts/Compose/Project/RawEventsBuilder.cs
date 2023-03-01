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
                if (!tg.GroupProperties.Editable)
                {
                    continue;
                }

                RawTimingGroup rawprop = tg.GroupProperties.ToRaw();

                List<ArcEvent> events = new List<ArcEvent>();

                events.AddRange(tg.GetEventType<Tap>());
                events.AddRange(tg.GetEventType<Hold>());
                events.AddRange(tg.GetEventType<Arc>());
                events.AddRange(tg.GetEventType<TimingEvent>());
                events.AddRange(Services.Gameplay.Chart.GetAll<CameraEvent>().Where(cam => cam.TimingGroup == tg.GroupNumber));
                events.AddRange(Services.Gameplay.Chart.GetAll<ScenecontrolEvent>().Where(sc => sc.TimingGroup == tg.GroupNumber));
                if (tg.ReferenceEvents != null)
                {
                    events.AddRange(tg.ReferenceEvents);
                }

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

                        if (a.Timing == b.Timing)
                        {
                            int atype = GetImportance(a);
                            int btype = GetImportance(b);
                            return atype.CompareTo(btype);
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
                                    Type = RawEventType.Timing,
                                    Timing = timing.Timing,
                                    TimingGroup = timing.TimingGroup,
                                    Bpm = timing.Bpm,
                                    Divisor = timing.Divisor,
                                };
                            case Tap tap:
                                return new RawTap
                                {
                                    Type = RawEventType.Tap,
                                    Timing = tap.Timing,
                                    TimingGroup = tap.TimingGroup,
                                    Lane = tap.Lane,
                                };
                            case Hold hold:
                                return new RawHold
                                {
                                    Type = RawEventType.Hold,
                                    Timing = hold.Timing,
                                    EndTiming = hold.EndTiming,
                                    TimingGroup = hold.TimingGroup,
                                    Lane = hold.Lane,
                                };
                            case Arc arc:
                                var ats = Services.Gameplay.Chart
                                    .GetAll<ArcTap>()
                                    .Where(at => at.Arc == arc)
                                    .Select(at => at.Timing);
                                return new RawArc
                                {
                                    Type = RawEventType.Arc,
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
                                    ArcTaps = ats.ToList(),
                                };
                            case CameraEvent cam:
                                return new RawCamera
                                {
                                    Type = RawEventType.Camera,
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
                                    Type = RawEventType.SceneControl,
                                    Timing = sc.Timing,
                                    TimingGroup = sc.TimingGroup,
                                    SceneControlTypeName = sc.Typename,
                                    Arguments = sc.Arguments,
                                };
                            case IncludeEvent incl:
                                return new RawInclude()
                                {
                                    Type = RawEventType.Include,
                                    Timing = incl.Timing,
                                    TimingGroup = incl.TimingGroup,
                                    File = incl.File,
                                };
                            case FragmentEvent frag:
                                return new RawFragment()
                                {
                                    Type = RawEventType.Fragment,
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

        private int GetImportance(ArcEvent a)
        {
            switch (a)
            {
                case TimingEvent time:
                    return 0;
                case IncludeEvent incl:
                    return 1;
                case FragmentEvent frag:
                    return 2;
                case Tap tap:
                    return 3;
                case Hold hold:
                    return 4;
                case Arc arc:
                    return 5;
                case CameraEvent cam:
                    return 6;
                case ScenecontrolEvent sc:
                    return 7;
                default:
                    return 8;
            }
        }
    }
}