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
    }
}