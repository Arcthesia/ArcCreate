using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public abstract class Trigger : ISerializableUnit
    {
        private TriggerChannel channel;

        public TriggerValueDispatch TriggerDispatch { get; set; }

        public void BindToChannel(TriggerChannel channel)
        {
            this.channel = channel;
        }

        public virtual void Destroy()
        {
        }

        public virtual void Reset()
        {
        }

        public abstract void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization);

        public Trigger DispatchValue(ValueChannel value, ValueChannel duration, string easing)
        {
            TriggerDispatch = new TriggerValueDispatch
            {
                Value = value,
                Duration = duration,
                EasingString = easing,
                Easing = Easing.FromString(easing),
            };

            return this;
        }

        public abstract void Poll(int timing);

        public abstract List<object> SerializeProperties(ScenecontrolSerialization serialization);

        protected virtual void Dispatch(int timing)
        {
            channel.Dispatch(new TriggerValueDispatchEvent
            {
                Value = TriggerDispatch.Value.ValueAt(timing),
                StartTiming = timing,
                Duration = (int)UnityEngine.Mathf.Max(1, TriggerDispatch.Duration.ValueAt(timing)),
                Easing = TriggerDispatch.Easing,
            });
        }
    }
}