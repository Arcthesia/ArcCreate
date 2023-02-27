using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class TriggerBuilder
    {
        public static Trigger OnJudgement()
            => new JudgementTrigger();

        public static Trigger Observe(ValueChannel channel)
            => new ObserveTrigger(channel);
    }
}