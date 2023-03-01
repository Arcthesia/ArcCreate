using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class SettingTriggerChannel : TriggerChannel
    {
        private float currentValue = 0;

        public SettingTriggerChannel()
            : base(new Trigger[0])
        {
        }

        public SettingTriggerChannel(Trigger[] triggers)
            : base(triggers)
        {
        }

        public override void Dispatch(TriggerValueDispatchEvent value)
        {
            currentValue = value.Value;
        }

        public override void Reset()
        {
            currentValue = 0;
        }

        protected override float CalculateAfterPoll(int timing)
        {
            return currentValue;
        }
    }
}