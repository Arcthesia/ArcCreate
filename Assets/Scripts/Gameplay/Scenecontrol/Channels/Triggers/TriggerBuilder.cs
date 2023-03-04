using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyAlias("Trigger")]
    [EmmyDoc("Class for creating different triggers")]
    [EmmySingleton]
    public class TriggerBuilder
    {
        [EmmyDoc("Creates a trigger that activates on judgement events")]
        public static JudgementTrigger Judgement()
            => new JudgementTrigger();

        [EmmyDoc("Creates a trigger that activates based on the value of a ValueChannel")]
        public static ObserveTrigger Observe(ValueChannel channel)
            => new ObserveTrigger(channel);
    }
}