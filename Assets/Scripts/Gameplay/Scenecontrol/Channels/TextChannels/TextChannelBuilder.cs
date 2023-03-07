using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyAlias("TextChannel")]
    [EmmyDoc("Class for creating text channels")]
    [EmmySingleton]
    public class TextChannelBuilder
    {
        [EmmyDoc("Creates an empty keyframe text channel")]
        public static KeyTextChannel Create()
        {
            return new KeyTextChannel();
        }

        [EmmyDoc("Creates a constant text channel")]
        public static TextChannel Constant(string value)
        {
            return new ConstantTextChannel(value);
        }

        [EmmyDoc("Creates a text channel that display a value. Beware of floating point precision")]
        public static TextChannel FromValue(ValueChannel channel, int maxLength = 10, int precision = 0)
        {
            return new ValueToTextChannel(channel, maxLength, precision);
        }
    }
}