using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class LoopingTriggerChannel : TriggerChannel
    {
        private TriggerValueDispatchEvent dispatching;
        private float currentValue = 0;
        private bool isDispatchComplete = true;

        public LoopingTriggerChannel()
            : base(new Trigger[0])
        {
        }

        public LoopingTriggerChannel(Trigger[] triggers)
            : base(triggers)
        {
        }

        public override void Dispatch(TriggerValueDispatchEvent value)
        {
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
                currentValue = dispatching.Value;
            }

            if (isDispatchComplete)
            {
                return BaseValue.ValueAt(timing) + currentValue;
            }
            else
            {
                float t = (float)(timing - dispatching.StartTiming) / dispatching.Duration;
                float dispatchingVal = dispatching.Easing.Invoke(0, dispatching.Value, t);
                return BaseValue.ValueAt(timing) + dispatchingVal;
            }
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield break;
        }
    }
}