using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class AccumulatingTriggerChannel : TriggerChannel
    {
        private TriggerValueDispatchEvent dispatching;
        private float currentValue;
        private bool isDispatchComplete = true;

        public AccumulatingTriggerChannel()
            : base(new Trigger[0])
        {
        }

        public AccumulatingTriggerChannel(Trigger[] triggers)
            : base(triggers)
        {
        }

        public override void Dispatch(TriggerValueDispatchEvent value)
        {
            if (!isDispatchComplete)
            {
                float t = (float)(value.StartTiming - dispatching.StartTiming) / value.Duration;
                currentValue += dispatching.Easing.Invoke(0, dispatching.Value, t);
            }

            isDispatchComplete = false;
            dispatching = value;
        }

        public override void Reset()
        {
            isDispatchComplete = true;
            currentValue = 0;
        }

        protected override float CalculateAfterPoll(int timing)
        {
            if (!isDispatchComplete && timing >= dispatching.StartTiming + dispatching.Duration)
            {
                isDispatchComplete = true;
                currentValue += dispatching.Value;
            }

            if (isDispatchComplete)
            {
                return BaseValue.ValueAt(timing) + currentValue;
            }
            else
            {
                float t = (float)(timing - dispatching.StartTiming) / dispatching.Duration;
                float dispatchingVal = dispatching.Easing.Invoke(0, dispatching.Value, t);
                return BaseValue.ValueAt(timing) + currentValue + dispatchingVal;
            }
        }
    }
}