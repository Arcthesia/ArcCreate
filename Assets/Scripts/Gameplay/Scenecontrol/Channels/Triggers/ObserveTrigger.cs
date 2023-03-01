using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class ObserveTrigger : Trigger
    {
        private ValueChannel target;
        private ValueChannel above;
        private ValueChannel below;

        private float lastDiffToAbove = float.MinValue;
        private float lastDiffToBelow = float.MaxValue;

        public ObserveTrigger()
        {
        }

        public ObserveTrigger(ValueChannel target)
        {
            this.target = target;
        }

        public ObserveTrigger GoAbove(ValueChannel above)
        {
            this.above = above;
            return this;
        }

        public ObserveTrigger GoBelow(ValueChannel below)
        {
            this.below = below;
            return this;
        }

        public ObserveTrigger Dispatch(ValueChannel value, ValueChannel duration = null, string easing = null)
        {
            TriggerValueDispatch dispatch = new TriggerValueDispatch
            {
                Value = value,
                Duration = duration ?? ValueChannel.ConstantOneChannel,
                Easing = Easing.FromString(easing),
            };

            return this;
        }

        public override void Poll(int timing)
        {
            float diffToAbove = target.ValueAt(timing) - above.ValueAt(timing);
            float diffToBelow = target.ValueAt(timing) - below.ValueAt(timing);

            if (lastDiffToAbove < 0 && diffToAbove >= 0)
            {
                Dispatch(timing);
            }

            if (lastDiffToBelow > 0 && diffToBelow <= 0)
            {
                Dispatch(timing);
            }

            lastDiffToAbove = diffToAbove;
            lastDiffToBelow = diffToBelow;
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                serialization.AddUnitAndGetId(target),
                serialization.AddUnitAndGetId(above),
                serialization.AddUnitAndGetId(below),
                serialization.AddUnitAndGetId(TriggerDispatch.Value),
                serialization.AddUnitAndGetId(TriggerDispatch.Duration),
                TriggerDispatch.EasingString,
            };
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            target = deserialization.GetUnitFromId((int)properties[0]) as ValueChannel;
            above = deserialization.GetUnitFromId((int)properties[1]) as ValueChannel;
            below = deserialization.GetUnitFromId((int)properties[2]) as ValueChannel;
            TriggerDispatch = new TriggerValueDispatch
            {
                Value = deserialization.GetUnitFromId((int)properties[3]) as ValueChannel,
                Duration = deserialization.GetUnitFromId((int)properties[4]) as ValueChannel,
                EasingString = (string)properties[5],
                Easing = Easing.FromString((string)properties[5]),
            };
        }
    }
}