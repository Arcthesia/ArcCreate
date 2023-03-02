using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class TextChannelBuilder
    {
        public static TextChannel Create()
        {
            return new KeyTextChannel();
        }

        public static TextChannel Constant(string value)
        {
            KeyTextChannel channel = new KeyTextChannel();
            channel.AddKey(int.MinValue, value);
            return channel;
        }

        public static TextChannel FromValue(ValueChannel channel, int maxLength = 10, int precision = 0)
        {
            return new ValueToTextChannel(channel, maxLength, precision);
        }
    }
}