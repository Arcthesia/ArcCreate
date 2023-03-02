using ArcCreate.Utility;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class StackingTriggerChannel : TriggerChannel
    {
        private readonly UnorderedList<TriggerValueDispatchEvent> dispatchingEvents
            = new UnorderedList<TriggerValueDispatchEvent>(5);

        private float currentValue;

        public StackingTriggerChannel()
            : base(new Trigger[0])
        {
        }

        public StackingTriggerChannel(Trigger[] triggers)
            : base(triggers)
        {
        }

        public override void Dispatch(TriggerValueDispatchEvent value)
        {
            dispatchingEvents.Add(value);
        }

        public override void Reset()
        {
            dispatchingEvents.Clear();
            currentValue = 0;
        }

        protected override float CalculateAfterPoll(int timing)
        {
            float partial = 0;
            for (int i = dispatchingEvents.Count - 1; i >= 0; i--)
            {
                TriggerValueDispatchEvent dispatching = dispatchingEvents[i];

                if (timing >= dispatching.StartTiming + dispatching.Duration)
                {
                    currentValue += dispatching.Value;
                    dispatchingEvents.RemoveAt(i);
                }
                else
                {
                    float t = (float)(timing - dispatching.StartTiming) / dispatching.Duration;
                    float dispatchingVal = dispatching.Easing.Invoke(0, dispatching.Value, t);
                    partial += dispatchingVal;
                }
            }

            return BaseValue.ValueAt(timing) + currentValue + partial;
        }
    }
}