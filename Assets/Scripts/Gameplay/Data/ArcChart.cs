using System.Collections.Generic;
using System.Linq;
using ArcCreate.ChartFormat;

namespace ArcCreate.Gameplay.Data
{
    public class ArcChart
    {
        public ArcChart(ChartReader reader)
        {
            AudioOffset = reader.AudioOffset;
            TotalTimingGroups = reader.TimingGroups.Count;
            TimingPointDensity = reader.TimingPointDensity;

            TimingGroups = reader.TimingGroups
                .Select(prop => new ChartTimingGroup { Properties = prop })
                .ToList();

            foreach (RawEvent e in reader.Events)
            {
                switch (e.Type)
                {
                    case RawEventType.Timing:
                        var timing = e as RawTiming;
                        TimingGroups[timing.TimingGroup].Timings.Add(
                            new TimingEvent()
                            {
                                TimingGroup = timing.TimingGroup,
                                Timing = timing.Timing,
                                Divisor = timing.Divisor,
                                Bpm = timing.Bpm,
                            });
                        break;

                    case RawEventType.Tap:
                        var tap = e as RawTap;
                        TimingGroups[tap.TimingGroup].Taps.Add(
                            new Tap()
                            {
                                TimingGroup = tap.TimingGroup,
                                Timing = tap.Timing,
                                Lane = tap.Lane,
                            });
                        break;

                    case RawEventType.Hold:
                        var hold = e as RawHold;
                        TimingGroups[hold.TimingGroup].Holds.Add(
                            new Hold()
                            {
                                TimingGroup = hold.TimingGroup,
                                EndTiming = hold.EndTiming,
                                Timing = hold.Timing,
                                Lane = hold.Lane,
                            });
                        break;

                    case RawEventType.Arc:
                        var raw = e as RawArc;
                        Arc arc = new Arc()
                        {
                            TimingGroup = raw.TimingGroup,
                            Color = raw.Color,
                            EndTiming = raw.EndTiming,
                            IsTrace = raw.IsTrace,
                            LineType = raw.LineType.ToArcLineType(),
                            Timing = raw.Timing,
                            XEnd = raw.XEnd,
                            XStart = raw.XStart,
                            YEnd = raw.YEnd,
                            YStart = raw.YStart,
                            Sfx = raw.Sfx,
                        };

                        if (raw.ArcTaps != null)
                        {
                            foreach (RawArcTap t in raw.ArcTaps)
                            {
                                ArcTap arctap = new ArcTap() { Timing = t.Timing, Arc = arc, TimingGroup = raw.TimingGroup };
                                TimingGroups[raw.TimingGroup].ArcTaps.Add(arctap);
                            }
                        }

                        TimingGroups[raw.TimingGroup].Arcs.Add(arc);
                        break;

                    case RawEventType.Camera:
                        var camera = e as RawCamera;
                        Cameras.Add(
                            new CameraEvent
                            {
                                TimingGroup = camera.TimingGroup,
                                Timing = camera.Timing,
                                Move = camera.Move,
                                Rotate = camera.Rotate,
                                CameraType = camera.CameraType.ToCameraType(),
                                Duration = camera.Duration,
                            });
                        break;

                    case RawEventType.SceneControl:
                        var sc = e as RawSceneControl;
                        SceneControls.Add(
                            new ScenecontrolEvent()
                            {
                                Timing = sc.Timing,
                                TimingGroup = sc.TimingGroup,
                                Typename = sc.SceneControlTypeName,
                                Arguments = sc.Arguments,
                            });
                        break;

                    case RawEventType.Include:
                        var incl = e as RawInclude;
                        TimingGroups[incl.TimingGroup].ReferenceEvents.Add(
                            new IncludeEvent()
                            {
                                Timing = incl.Timing,
                                TimingGroup = incl.TimingGroup,
                                File = incl.File,
                            });
                        break;

                    case RawEventType.Fragment:
                        var frag = e as RawFragment;
                        TimingGroups[frag.TimingGroup].ReferenceEvents.Add(
                            new FragmentEvent()
                            {
                                Timing = frag.Timing,
                                TimingGroup = frag.TimingGroup,
                                File = frag.File,
                            });
                        break;
                }
            }

            if (Settings.MirrorNotes.Value)
            {
                foreach (ChartTimingGroup tg in TimingGroups)
                {
                    foreach (var tap in tg.Taps)
                    {
                        tap.Lane = 5 - tap.Lane;
                    }

                    foreach (var hold in tg.Holds)
                    {
                        hold.Lane = 5 - hold.Lane;
                    }

                    foreach (var arc in tg.Arcs)
                    {
                        arc.XStart = 1 - arc.XStart;
                        arc.XEnd = 1 - arc.XEnd;
                        arc.Color = arc.Color == 0 ? 1 : (arc.Color == 1 ? 0 : arc.Color);
                    }
                }
            }
        }

        public int AudioOffset { get; private set; }

        public float TimingPointDensity { get; private set; }

        public int TotalTimingGroups { get; private set; }

        public List<ChartTimingGroup> TimingGroups { get; private set; }

        public List<CameraEvent> Cameras { get; private set; } = new List<CameraEvent>();

        public List<ScenecontrolEvent> SceneControls { get; private set; } = new List<ScenecontrolEvent>();
    }
}
