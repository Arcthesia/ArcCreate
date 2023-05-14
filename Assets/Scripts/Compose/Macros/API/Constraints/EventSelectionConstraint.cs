using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyDoc("Constraint for filtering events selection.")]
    [EmmyGroup("Macros")]
    public class EventSelectionConstraint
    {
        private int? timingGroup = null;
        private int? fromTiming = null;
        private int? toTiming = null;
        private EventType inputType = EventType.Any;
        private DynValue customCheck = null;
        private string customMessage = "";
        private EventSelectionConstraint unionWith = null;

        public enum EventType
        {
            Any,
            Tap,
            Hold,
            Arc,
            SolidArc,
            VoidArc,
            ArcTap,
            Timing,
            Camera,
            Scenecontrol,
            Floor,
            Sky,
            Short,
            Long,
            Judgeable,
        }

        [MoonSharpHidden]
        public EventType Type
        {
            get => inputType;
        }

        [EmmyDoc("Create a new constraint.")]
        public static EventSelectionConstraint Create()
        {
            return new EventSelectionConstraint();
        }

        [EmmyDoc("Set the event type to filter for")]
        public EventSelectionConstraint SetType(
#pragma warning disable
        [EmmyChoice("any", "tap", "hold", "arc",
                    "solidarc", "voidarc", "trace", "arctap",
                    "timing", "camera", "floor", "sky",
                    "short", "long", "judgeable")]
        string type)
#pragma warning restore
        {
            switch (type.ToLower())
            {
                case "any":
                    return Any();
                case "tap":
                    return Tap();
                case "hold":
                    return Hold();
                case "arc":
                    return Arc();
                case "solidarc":
                    return SolidArc();
                case "voidarc":
                    return VoidArc();
                case "trace":
                    return Trace();
                case "arctap":
                    return ArcTap();
                case "timing":
                    return Timing();
                case "camera":
                    return Camera();
                case "scenecontrol":
                    return Scenecontrol();
                case "floor":
                    return Floor();
                case "sky":
                    return Sky();
                case "short":
                    return Short();
                case "long":
                    return Long();
                case "judgeable":
                    return Judgeable();
                default:
                    return this;
            }
        }

        [EmmyDoc("Filter for all event types.")]
        public EventSelectionConstraint Any()
        {
            inputType = EventType.Any;
            return this;
        }

        [EmmyDoc("Filter for timing events.")]
        public EventSelectionConstraint Timing()
        {
            inputType = EventType.Timing;
            return this;
        }

        [EmmyDoc("Filter for camera events.")]
        public EventSelectionConstraint Camera()
        {
            inputType = EventType.Camera;
            return this;
        }

        [EmmyDoc("Filter for scenecontrol events.")]
        public EventSelectionConstraint Scenecontrol()
        {
            inputType = EventType.Scenecontrol;
            return this;
        }

        [EmmyDoc("Filter for tap events.")]
        public EventSelectionConstraint Tap()
        {
            inputType = EventType.Tap;
            return this;
        }

        [EmmyDoc("Filter for hold events.")]
        public EventSelectionConstraint Hold()
        {
            inputType = EventType.Hold;
            return this;
        }

        [EmmyDoc("Filter for arc and trace events.")]
        public EventSelectionConstraint Arc()
        {
            inputType = EventType.Arc;
            return this;
        }

        [EmmyDoc("Filter for trace events.")]
        public EventSelectionConstraint VoidArc()
        {
            inputType = EventType.VoidArc;
            return this;
        }

        [EmmyDoc("Filter for trace events.")]
        public EventSelectionConstraint Trace()
        {
            inputType = EventType.VoidArc;
            return this;
        }

        [EmmyDoc("Filter for arc events (without trace events).")]
        public EventSelectionConstraint SolidArc()
        {
            inputType = EventType.SolidArc;
            return this;
        }

        [EmmyDoc("Filter for arctap events.")]
        public EventSelectionConstraint Arctap()
        {
            inputType = EventType.ArcTap;
            return this;
        }

        [EmmyDoc("Filter for arctap events.")]
        public EventSelectionConstraint ArcTap() => Arctap();

        [EmmyDoc("Filter for tap and hold events.")]
        public EventSelectionConstraint Floor()
        {
            inputType = EventType.Floor;
            return this;
        }

        [EmmyDoc("Filter for hold, arc and trace events.")]
        public EventSelectionConstraint Long()
        {
            inputType = EventType.Long;
            return this;
        }

        [EmmyDoc("Filter for tap and arctap events.")]
        public EventSelectionConstraint Short()
        {
            inputType = EventType.Short;
            return this;
        }

        [EmmyDoc("Filter for arctap, arc and trace events.")]
        public EventSelectionConstraint Sky()
        {
            inputType = EventType.Sky;
            return this;
        }

        [EmmyDoc("Filter for tap, arctap, hold, arc events (without trace events).")]
        public EventSelectionConstraint Judgeable()
        {
            inputType = EventType.Judgeable;
            return this;
        }

        [EmmyDoc("Filter for notes with timing larger than the provided value.")]
        public EventSelectionConstraint FromTiming(int timing)
        {
            fromTiming = timing;
            return this;
        }

        [EmmyDoc("Filter for notes with smaller than the provided value.")]
        public EventSelectionConstraint ToTiming(int timing)
        {
            toTiming = timing;
            return this;
        }

        [EmmyDoc("Filter for notes belonging to the group-th timing group.")]
        public EventSelectionConstraint OfTimingGroup(int group)
        {
            timingGroup = group;
            return this;
        }

        [EmmyDoc("Set a custom filter.")]
        public EventSelectionConstraint Custom(DynValue function, string message = "Invalid")
        {
            customCheck = function;
            customMessage = message;
            return this;
        }

        [EmmyDoc("Also include notes that passes the provided constraint.")]
        public EventSelectionConstraint Union(EventSelectionConstraint constraint)
        {
            unionWith = constraint;
            return this;
        }

        [EmmyDoc("Get the constraint description.")]
        public string GetConstraintDescription()
        {
            string result = "";

            switch (inputType)
            {
                case EventType.Tap:
                    result = "Must select tap notes";
                    break;
                case EventType.Hold:
                    result = "Must select hold notes";
                    break;
                case EventType.SolidArc:
                    result = "Must select arc notes";
                    break;
                case EventType.VoidArc:
                    result = "Must select trace notes";
                    break;
                case EventType.ArcTap:
                    result = "Must select arctap notes";
                    break;
                case EventType.Timing:
                    result = "Must select timing events";
                    break;
                case EventType.Camera:
                    result = "Must select camera events";
                    break;
                case EventType.Scenecontrol:
                    result = "Must select scenecontrol events";
                    break;
                case EventType.Floor:
                    result = "Must select tap or hold notes";
                    break;
                case EventType.Sky:
                    result = "Must select arc, trace or arctap notes";
                    break;
                case EventType.Arc:
                    result = "Must select arc or trace notes";
                    break;
                case EventType.Short:
                    result = "Must select tap or arctap notes";
                    break;
                case EventType.Long:
                    result = "Must select hold, arc or trace notes";
                    break;
                case EventType.Judgeable:
                    result = "Must select tap, hold, arc or arctap notes";
                    break;
                case EventType.Any:
                    result = "Must select any notes";
                    break;
            }

            if (timingGroup != null)
            {
                result += $" of timing group {timingGroup}";
            }

            if (fromTiming != null && toTiming != null)
            {
                result += $" with timing included in the range from {fromTiming} to {toTiming}";
            }
            else if (fromTiming != null)
            {
                result += $" with timing greater than or equal to {fromTiming}";
            }
            else if (toTiming != null)
            {
                result += $" with timing smaller than or equal to {toTiming}";
            }

            if (unionWith != null)
            {
                result += "\nOR " + unionWith.GetConstraintDescription();
            }

            return result;
        }

        [MoonSharpHidden]
        public (bool valid, string invalidReason) GetValidityDetail(LuaChartEvent value)
        {
            if (customCheck != null)
            {
                return (customCheck.Function.Call(value).Boolean, customMessage);
            }

            string message = GetConstraintDescription();
            if (inputType == EventType.Any)
            {
                return (true, "");
            }

            if (timingGroup != null && value.TimingGroup != timingGroup)
            {
                return (false, message);
            }

            if (unionWith != null)
            {
                bool unionResult = unionWith.GetValidityDetail(value).valid;
                if (unionResult)
                {
                    return (true, "");
                }
            }

            switch (inputType)
            {
                case EventType.Tap:
                    if (!(value is LuaTap))
                    {
                        return (false, message);
                    }

                    break;
                case EventType.Hold:
                    if (!(value is LuaHold))
                    {
                        return (false, message);
                    }

                    break;
                case EventType.Arc:
                    if (!(value is LuaArc))
                    {
                        return (false, message);
                    }

                    break;
                case EventType.SolidArc:
                    if (!(value is LuaArc) || (value as LuaArc).IsTrace)
                    {
                        return (false, message);
                    }

                    break;
                case EventType.VoidArc:
                    if (!(value is LuaArc) || !(value as LuaArc).IsTrace)
                    {
                        return (false, message);
                    }

                    break;
                case EventType.ArcTap:
                    if (!(value is LuaArcTap))
                    {
                        return (false, message);
                    }

                    break;
                case EventType.Timing:
                    if (!(value is LuaTiming))
                    {
                        return (false, message);
                    }

                    break;
                case EventType.Camera:
                    if (!(value is LuaCamera))
                    {
                        return (false, message);
                    }

                    break;
                case EventType.Scenecontrol:
                    if (!(value is LuaScenecontrol))
                    {
                        return (false, message);
                    }

                    break;
                case EventType.Floor:
                    if (!(value is LuaTap) && !(value is LuaHold))
                    {
                        return (false, message);
                    }

                    break;
                case EventType.Sky:
                    if (!(value is LuaArc) && !(value is LuaArcTap))
                    {
                        return (false, message);
                    }

                    break;
                case EventType.Short:
                    if (!(value is LuaArcTap) && !(value is LuaTap))
                    {
                        return (false, message);
                    }

                    break;
                case EventType.Long:
                    if (!(value is LuaHold) && !(value is LuaArc))
                    {
                        return (false, message);
                    }

                    break;
                case EventType.Judgeable:
                    if (!(value is LuaTap) && !(value is LuaHold)
                    && (!(value is LuaArc) || (value as LuaArc).IsTrace)
                    && !(value is LuaArcTap))
                    {
                        return (false, message);
                    }

                    break;
            }

            if (fromTiming != null && value.Timing < fromTiming)
            {
                return (false, message);
            }

            if (toTiming != null && value.Timing > toTiming)
            {
                return (false, message);
            }

            return (true, "");
        }

        [EmmyDoc("Check if an event satisfy this constraint.")]
        public bool CheckEvent(LuaChartEvent value) => GetValidityDetail(value).valid;
    }
}