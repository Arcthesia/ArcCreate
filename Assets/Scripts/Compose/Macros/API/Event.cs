using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Lua;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmySingleton]
    [EmmyGroup("Macros")]
    public class Event
    {
        [EmmyDoc("Create an arc event data.")]
        public static LuaArc Arc(
            float timing,
            float startX,
            float startY,
            float endTiming,
            float endX,
            float endY,
            bool isTrace = false,
            int color = 0,
            [EmmyChoice("b", "s", "si", "so", "sisi", "soso", "siso", "sosi")]
            string type = "s",
            int timingGroup = 0,
            string sfx = "none")
        {
            if (timingGroup >= Services.Gameplay.Chart.TimingGroups.Count || timingGroup < 0)
            {
                throw new Exception("Invalid timingGroup value");
            }

            return new LuaArc
            {
                Timing = timing,
                EndTiming = endTiming,
                StartXY = new XY(startX, startY),
                EndXY = new XY(endX, endY),
                Color = color,
                IsTrace = isTrace,
                ArcType = type.ToArcLineType(),
                TimingGroup = timingGroup,
                Sfx = sfx,
            };
        }

        [EmmyDoc("Create an arc event data.")]
        public static LuaArc Arc(
            float timing,
            XY startXY,
            float endTiming,
            XY endXY,
            bool isTrace = false,
            int color = 0,
            [EmmyChoice("b", "s", "si", "so", "sisi", "soso", "siso", "sosi")]
            string type = "s",
            int timingGroup = 0,
            string sfx = "none")
        {
            return Arc(
                timing,
                startXY.X,
                startXY.Y,
                endTiming,
                endXY.X,
                endXY.Y,
                isTrace,
                color,
                type,
                timingGroup,
                sfx);
        }

        [EmmyDoc("Create a tap event data.")]
        public static LuaTap Tap(int timing, float lane, int timingGroup = 0)
        {
            if (timingGroup >= Services.Gameplay.Chart.TimingGroups.Count || timingGroup < 0)
            {
                throw new Exception("Invalid timingGroup value");
            }

            return new LuaTap
            {
                Timing = timing,
                Lane = lane,
                TimingGroup = timingGroup,
            };
        }

        [EmmyDoc("Create a hold event data.")]
        public static LuaHold Hold(int timing, int endTiming, float lane, int timingGroup = 0)
        {
            if (timingGroup >= Services.Gameplay.Chart.TimingGroups.Count || timingGroup < 0)
            {
                throw new Exception("Invalid timingGroup value");
            }

            return new LuaHold
            {
                Timing = timing,
                EndTiming = endTiming,
                Lane = lane,
                TimingGroup = timingGroup,
            };
        }

        [EmmyDoc("Create an arctap event data.")]
        public static LuaArcTap ArcTap(int timing, float width, LuaArc arc)
        {
            return new LuaArcTap
            {
                Timing = timing,
                Arc = arc,
                Width = width,
            };
        }

        [EmmyDoc("Create an arctap event data.")]
        public static LuaArcTap Arctap(int timing, float width, LuaArc arc) => ArcTap(timing, width, arc);

        [EmmyDoc("Create a timing event data.")]
        public static LuaTiming Timing(int timing, float bpm, float divisor, int timingGroup = 0)
        {
            if (timingGroup >= Services.Gameplay.Chart.TimingGroups.Count || timingGroup < 0)
            {
                throw new Exception("Invalid timingGroup value");
            }

            return new LuaTiming
            {
                Timing = timing,
                Bpm = bpm,
                Divisor = divisor,
                TimingGroup = timingGroup,
            };
        }

        [EmmyDoc("Create a camera event data.")]
        public static LuaCamera Camera(
            int timing,
            XYZ move,
            XYZ rotate,
            [EmmyChoice("l", "qi", "qo", "reset", "s")]
            string type = "reset",
            int duration = 1,
            int timingGroup = 0)
        {
            if (timingGroup >= Services.Gameplay.Chart.TimingGroups.Count || timingGroup < 0)
            {
                throw new Exception("Invalid timingGroup value");
            }

            return new LuaCamera
            {
                Timing = timing,
                Move = move,
                Rotate = rotate,
                CameraType = type.ToCameraType(),
                Duration = duration,
                TimingGroup = timingGroup,
            };
        }

        [EmmyDoc("Create a camera event data.")]
        public static LuaCamera Camera(
            int timing,
            float x = 0,
            float y = 0,
            float z = 0,
            float rx = 0,
            float ry = 0,
            float rz = 0,
            [EmmyChoice("l", "qi", "qo", "reset", "s")]
            string type = "reset",
            int duration = 1,
            int timingGroup = 0)
        {
            return Camera(
                timing,
                new XYZ(x, y, z),
                new XYZ(rx, ry, rz),
                type,
                duration,
                timingGroup);
        }

        [EmmyDoc("Create a scenecontrol event data.")]
        public static LuaScenecontrol Scenecontrol(
            int timing,
            string type,
            int timingGroup,
            params DynValue[] args)
        {
            object[] obj = args.Select(arg =>
            {
                if (arg.Number != 0 || arg.String == "0")
                {
                    UnityEngine.Debug.Log(0);
                    return (object)(float)arg.Number;
                }

                return arg.String;
            }).ToArray();

            if (timingGroup >= Services.Gameplay.Chart.TimingGroups.Count || timingGroup < 0)
            {
                throw new Exception("Invalid timingGroup value");
            }

            return new LuaScenecontrol
            {
                Timing = timing,
                Type = type,
                Args = obj,
                TimingGroup = timingGroup,
            };
        }

        [EmmyDoc("Create a timing group object. Properties are defined with a string whose format is the same as .aff chart format."
               + "You must use LuaTimingGroup.save() in order to add this timing group to the chart.")]
        public static LuaTimingGroup CreateTimingGroup(string properties = "")
        {
            RawTimingGroup prop = RawTimingGroup.Parse(properties).UnwrapOrElse(e => throw new Exception(e.Message));
            prop.File = Services.Gameplay.Chart.GetTimingGroup(0).GroupProperties.FileName;
            return new LuaTimingGroup(prop);
        }

        [EmmyDoc("Deprecated, use Event.CreateTimingGroup() instead. Create a timing group object. Properties are defined with a string whose format is the same as .aff chart format.")]
        public static LuaTimingGroup CreateTimingGroupProperty(string properties = "")
            => CreateTimingGroup(properties);

        [EmmyDoc("Deprecated. This method does not support the latest timing group properties, use Event.CreateTimingGroup() instead."
               + "Create a timing group object. Properties are defined with a table.")]
        public static LuaTimingGroup CreateTimingGroupProperty(Dictionary<string, DynValue> properties)
        {
            string name = null;
            bool noInput = false;
            bool noClip = false;
            bool noHeightIndicator = false;
            bool noShadow = false;
            bool noHead = false;
            bool noArcCap = false;
            bool fadingHolds = false;
            float arcResolution = 1;
            float angleX = 0;
            float angleY = 0;
            string side = "none";
            string file = Services.Gameplay.Chart.GetTimingGroup(0).GroupProperties.FileName;

            foreach (var pair in properties)
            {
                DynValue val = pair.Value;
                switch (pair.Key.ToLower())
                {
                    case "name":
                        name = val.String;
                        break;

                    case "noinput":
                        noInput = val.Boolean;
                        break;

                    case "noclip":
                        noClip = val.Boolean;
                        break;

                    case "noheightindicator":
                        noHeightIndicator = val.Boolean;
                        break;

                    case "noshadow":
                        noShadow = val.Boolean;
                        break;

                    case "nohead":
                        noHead = val.Boolean;
                        break;

                    case "noarccap":
                        noArcCap = val.Boolean;
                        break;

                    case "fadingholds":
                        fadingHolds = val.Boolean;
                        break;

                    case "arcresolution":
                        arcResolution = (float)val.Number;
                        break;

                    case "anglex":
                        angleX = (float)val.Number;
                        break;

                    case "angley":
                        angleY = (float)val.Number;
                        break;

                    case "side":
                        side = val.String;
                        break;

                    case "file":
                        file = val.String;
                        break;
                }
            }

            LuaTimingGroup tg = new LuaTimingGroup
            {
                Name = name,
                NoInput = noInput,
                NoClip = noClip,
                NoHeightIndicator = noHeightIndicator,
                NoShadow = noShadow,
                NoHead = noHead,
                NoArcCap = noArcCap,
                FadingHolds = fadingHolds,
                ArcResolution = arcResolution,
                AngleX = angleX,
                AngleY = angleY,
                Side = side,
                File = file,
            };

            return tg;
        }


        [EmmyDoc("Not recommended, use LuaTimingGroup.save() instead. Add a timing group to the current chart.")]
        public static void AddTimingGroup(LuaTimingGroup properties = null)
        {
            int num = Services.Gameplay.Chart.TimingGroups.Count;
            var tg = Services.Gameplay.Chart.GetTimingGroup(num);

            if (properties != null)
            {
                tg.SetProperties(properties.ToProperty());
            }
        }

        [EmmyDoc("Not recommended, use LuaTimingGroup.save() instead. Change an existing timing group's properties. Properties are defined with a property string, similar to that of .aff chart format")]
        public static void SetTimingGroupProperty(int group, LuaTimingGroup properties = null)
        {
            if (group == 0)
            {
                return;
            }

            Services.Gameplay.Chart.GetTimingGroup(group).SetProperties(properties.ToProperty());
        }

        [EmmyDoc("Get the n-th timing group of the current chart.")]
        public static LuaTimingGroup GetTimingGroup(int group)
        {
            var tg = Services.Gameplay.Chart.GetTimingGroup(group);
            RawTimingGroup prop = tg.GroupProperties.ToRaw();
            var result = new LuaTimingGroup(prop)
            {
                Instance = tg
            };

            return result;
        }

        [EmmyDoc("Get the current note selection. If a constraint is provided, then only notes that fit the constraint is returned.")]
        public static EventSelectionRequest GetCurrentSelection(EventSelectionConstraint constraint = null)
        {
            var events = ConvertAll(Services.Selection.SelectedNotes);
            if (constraint == null)
            {
                return new EventSelectionRequest
                {
                    Result = events,
                    Complete = true,
                };
            }
            else
            {
                var result = new Dictionary<string, List<LuaChartEvent>>();
                foreach (string key in events.Keys)
                {
                    result.Add(key, events[key].Where(e => constraint.GetValidityDetail(e).valid).ToList());
                }

                return new EventSelectionRequest
                {
                    Result = result,
                    Complete = true,
                };
            }
        }

        [EmmyDoc("Set the selections to the provided notes. Non-selectable events (such as timing events) are ignored.")]
        public static void SetSelection(Table notes)
        {
            List<Note> toSelect = new List<Note>();
            foreach (DynValue dynValue in notes.Values)
            {
                ArcEvent e = dynValue.ToObject<LuaChartEvent>().Instance;
                if (e is Note)
                {
                    toSelect.Add(e as Note);
                }
            }

            Services.Selection.SetSelection(toSelect);
        }

        [EmmyDoc("Query for all notes. If a constraint is provided, then only notes that fit the constraint is returned.")]
        public static EventSelectionRequest Query(EventSelectionConstraint constraint = null)
        {
            List<ArcEvent> events = new List<ArcEvent>();
            events.AddRange(Services.Gameplay.Chart.GetAll<Tap>());
            events.AddRange(Services.Gameplay.Chart.GetAll<Hold>());
            events.AddRange(Services.Gameplay.Chart.GetAll<Arc>());
            events.AddRange(Services.Gameplay.Chart.GetAll<ArcTap>());
            events.AddRange(Services.Gameplay.Chart.GetAll<TimingEvent>());
            events.AddRange(Services.Gameplay.Chart.GetAll<CameraEvent>());
            events.AddRange(Services.Gameplay.Chart.GetAll<ScenecontrolEvent>());

            Dictionary<string, List<LuaChartEvent>> luaEvents = ConvertAll(events);
            if (constraint == null)
            {
                return new EventSelectionRequest
                {
                    Result = luaEvents,
                    Complete = true,
                };
            }
            else
            {
                Dictionary<string, List<LuaChartEvent>> result = new Dictionary<string, List<LuaChartEvent>>();
                foreach (string key in luaEvents.Keys)
                {
                    result.Add(key, luaEvents[key].Where(e => constraint.GetValidityDetail(e).valid).ToList());
                }

                return new EventSelectionRequest
                {
                    Result = result,
                    Complete = true,
                };
            }
        }

        [MoonSharpHidden]
        public static Dictionary<string, List<LuaChartEvent>> ConvertAll(IEnumerable<ArcEvent> events)
        {
            Dictionary<string, List<LuaChartEvent>> result = new Dictionary<string, List<LuaChartEvent>>()
            {
                { "tap", new List<LuaChartEvent>() },
                { "hold", new List<LuaChartEvent>() },
                { "arc", new List<LuaChartEvent>() },
                { "arctap", new List<LuaChartEvent>() },
                { "timing", new List<LuaChartEvent>() },
                { "camera", new List<LuaChartEvent>() },
                { "scenecontrol", new List<LuaChartEvent>() },
            };

            Dictionary<Arc, LuaArc> arcs = new Dictionary<Arc, LuaArc>();

            var sorted = events.ToList();
            sorted.Sort((a, b) => a.Timing.CompareTo(b.Timing));

            foreach (ArcEvent e in sorted)
            {
                if (e is Arc n)
                {
                    LuaArc a = Arc(
                        n.Timing,
                        n.XStart,
                        n.YStart,
                        n.EndTiming,
                        n.XEnd,
                        n.YEnd,
                        n.IsTrace,
                        n.Color,
                        n.LineType.ToLineTypeString(),
                        n.TimingGroup,
                        n.Sfx);
                    a.SetInstance(n);
                    result["arc"].Add(a);
                    arcs.Add(n, a);
                }
            }

            foreach (ArcEvent e in sorted)
            {
                switch (e)
                {
                    case Tap n:
                        LuaTap t = Tap(n.Timing, n.Lane, n.TimingGroup);
                        t.SetInstance(n);
                        result["tap"].Add(t);
                        break;

                    case Hold n:
                        LuaHold h = Hold(n.Timing, n.EndTiming, n.Lane, n.TimingGroup);
                        h.SetInstance(n);
                        result["hold"].Add(h);
                        break;

                    case ArcTap n:
                        LuaArcTap at;
                        if (arcs.Keys.Contains(n.Arc))
                        {
                            at = ArcTap(n.Timing, n.Width, arcs[n.Arc]);
                        }
                        else
                        {
                            LuaArc a = Arc(
                                n.Arc.Timing,
                                n.Arc.XStart,
                                n.Arc.YStart,
                                n.Arc.EndTiming,
                                n.Arc.XEnd,
                                n.Arc.YEnd,
                                n.Arc.IsTrace,
                                n.Arc.Color,
                                n.Arc.LineType.ToLineTypeString(),
                                n.Arc.TimingGroup,
                                n.Arc.Sfx);
                            arcs.Add(n.Arc, a);
                            at = ArcTap(n.Timing, n.Width, a);
                        }

                        at.SetInstance(n);
                        result["arctap"].Add(at);
                        break;

                    case TimingEvent n:
                        LuaTiming tm = Timing(n.Timing, n.Bpm, n.Divisor, n.TimingGroup);
                        tm.SetInstance(n);
                        result["timing"].Add(tm);
                        break;

                    case CameraEvent n:
                        LuaCamera cam = Camera(
                            n.Timing,
                            n.Move.x,
                            n.Move.y,
                            n.Move.z,
                            n.Rotate.x,
                            n.Rotate.y,
                            n.Rotate.z,
                            n.CameraType.ToCameraString(),
                            n.Duration,
                            n.TimingGroup);
                        cam.SetInstance(n);
                        result["camera"].Add(cam);
                        break;

                    case ScenecontrolEvent n:
                        LuaScenecontrol sc = new LuaScenecontrol
                        {
                            Timing = n.Timing,
                            Type = n.Typename,
                            TimingGroup = n.TimingGroup,
                            Args = n.Arguments.ToArray(),
                        };
                        sc.SetInstance(n);
                        result["scenecontrol"].Add(sc);
                        break;
                }
            }

            return result;
        }
    }
}