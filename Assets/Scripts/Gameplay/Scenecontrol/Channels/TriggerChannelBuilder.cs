using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyAlias("TriggerChannel")]
    [EmmyDoc("Class for creating different trigger channels")]
    [EmmySingleton]
    public class TriggerChannelBuilder
    {
        [EmmyDoc("Creates an accumulating trigger channel. Ongoing trigger event will be halted upon a new one arrives.")]
        public static AccumulatingTriggerChannel Accumulate(params Trigger[] triggers)
            => new AccumulatingTriggerChannel(triggers);

        [EmmyDoc("Creates a looping trigger channel. Channel's value will be reset once a trigger event arrives.")]
        public static LoopingTriggerChannel Loop(params Trigger[] triggers)
            => new LoopingTriggerChannel(triggers);

        [EmmyDoc("Creates a stacking trigger channel. Similar to accumulating channels, but new trigger event will not halt ongoing ones")]
        public static StackingTriggerChannel Stack(params Trigger[] triggers)
            => new StackingTriggerChannel(triggers);

        [EmmyDoc("Creates a trigger channel that instantly set it's value to the trigger event's value, ignoring duration.")]
        public static SettingTriggerChannel SetTo(params Trigger[] triggers)
            => new SettingTriggerChannel(triggers);
    }
}