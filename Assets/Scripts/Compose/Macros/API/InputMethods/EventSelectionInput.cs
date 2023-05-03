using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyDoc("Class for requesting note selection input from user.")]
    [EmmyGroup("Macros")]
    public class EventSelectionInput
    {
        [EmmyDoc("Request for selection of a single event.")]
        public static EventSelectionRequest RequestSingleEvent(EventSelectionConstraint constraint, string notification = null)
        {
            EventSelectionRequest request = new EventSelectionRequest();
            if (notification != null)
            {
                Services.Popups.Notify(Popups.Severity.Info, notification);
            }
            else
            {
                Services.Popups.Notify(Popups.Severity.Info, constraint.GetConstraintDescription());
            }

            Services.Macros.RequestSelection(constraint, request, true);
            return request;
        }

        [EmmyDoc("Request for selection of multiple events.")]
        public static EventSelectionRequest RequestEvents(EventSelectionConstraint constraint, string notification = null)
        {
            EventSelectionRequest request = new EventSelectionRequest();
            if (notification != null)
            {
                Services.Popups.Notify(Popups.Severity.Info, notification);
            }
            else
            {
                Services.Popups.Notify(Popups.Severity.Info, constraint.GetConstraintDescription());
            }

            Services.Macros.RequestSelection(constraint, request, false);
            return request;
        }
    }
}