using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public abstract class Trigger : ISerializableUnit
    {
        private TriggerChannel channel;

        [MoonSharpHidden]
        public TriggerValueDispatch TriggerDispatch { get; set; }

        [MoonSharpHidden]
        public void BindToChannel(TriggerChannel channel)
        {
            this.channel = channel;
        }

        [MoonSharpHidden]
        public abstract void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization);

        [MoonSharpHidden]
        public abstract void Poll(int timing);

        [MoonSharpHidden]
        public abstract List<object> SerializeProperties(ScenecontrolSerialization serialization);

        [MoonSharpHidden]
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