using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyAlias("StringChannel")]
    [EmmyDoc("Class for creating string channels")]
    [EmmySingleton]
    public class StringChannelBuilder
    {
        [EmmyDoc("Creates an empty keyframe string channel")]
        public static KeyStringChannel Create()
        {
            return new KeyStringChannel();
        }

        [EmmyDoc("Creates constant string channel")]
        public static StringChannel Constant(string value)
        {
            KeyStringChannel channel = new KeyStringChannel();
            channel.AddKey(int.MinValue, value);
            return channel;
        }
    }
}