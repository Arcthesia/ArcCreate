using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class StringChannelBuilder
    {
        public static StringChannel Create()
        {
            return new KeyStringChannel();
        }

        public static StringChannel Constant(string value)
        {
            KeyStringChannel channel = new KeyStringChannel();
            channel.AddKey(int.MinValue, value);
            return channel;
        }
    }
}