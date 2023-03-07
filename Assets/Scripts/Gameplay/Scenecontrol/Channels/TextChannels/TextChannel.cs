using System.Collections.Generic;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Channel defining text at any given input value. Used for controlling TextController's text content")]
    public abstract class TextChannel : ISerializableUnit
    {
        public abstract int MaxLength { get; }

        [MoonSharpHidden]
        public abstract char[] ValueAt(int timing, out int length, out bool hasChanged);

        [MoonSharpHidden]
        public abstract List<object> SerializeProperties(ScenecontrolSerialization serialization);

        [MoonSharpHidden]
        public abstract void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization);
    }
}