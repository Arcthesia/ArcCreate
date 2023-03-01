using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class TriggerChannelBuilder
    {
        public static TriggerChannel Accumulate(params Trigger[] triggers)
            => new AccumulatingTriggerChannel(triggers);

        public static TriggerChannel Loop(params Trigger[] triggers)
            => new LoopingTriggerChannel(triggers);

        public static TriggerChannel Stack(params Trigger[] triggers)
            => new StackingTriggerChannel(triggers);

        public static TriggerChannel SetTo(params Trigger[] triggers)
            => new SettingTriggerChannel(triggers);
    }
}