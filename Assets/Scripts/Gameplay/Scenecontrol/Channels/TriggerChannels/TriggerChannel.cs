using System.Collections.Generic;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public abstract class TriggerChannel : ValueChannel
    {
        private Trigger[] triggers;

        public TriggerChannel(Trigger[] triggers)
        {
            BaseValue = ConstantZeroChannel;
            this.triggers = triggers;
            foreach (var trigger in triggers)
            {
                trigger.BindToChannel(this);
            }
        }

        protected ValueChannel BaseValue { get; private set; }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            triggers = new Trigger[properties.Count];
            for (int i = 0; i < triggers.Length; i++)
            {
                triggers[i] = deserialization.GetUnitFromId((int)properties[i]) as Trigger;
            }
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            List<object> result = new List<object>();
            foreach (var trigger in triggers)
            {
                result.Add(serialization.AddUnitAndGetId(trigger));
            }

            return result;
        }

        [EmmyDoc("Sets the base value of this channel.")]
        public TriggerChannel SetBaseValue(ValueChannel value)
        {
            BaseValue = value;
            return this;
        }

        public override float ValueAt(int timing)
        {
            foreach (var trigger in triggers)
            {
                trigger.Poll(timing);
            }

            return CalculateAfterPoll(timing);
        }

        [MoonSharpHidden]
        public abstract void Dispatch(TriggerValueDispatchEvent value);

        protected abstract float CalculateAfterPoll(int timing);
    }
}