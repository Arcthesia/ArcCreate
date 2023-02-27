using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public abstract class Trigger : SerializableUnit
    {
        private TriggerChannel channel;

        public TriggerValueDispatch TriggerDispatch { get; set; }

        public void BindToChannel(TriggerChannel channel)
        {
            this.channel = channel;
        }

        public abstract void Poll(int timing);

        protected void Dispatch(int timing)
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