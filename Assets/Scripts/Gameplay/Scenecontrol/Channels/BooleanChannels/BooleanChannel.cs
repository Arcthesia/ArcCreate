using System.Collections.Generic;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [SerializationExempt]
    [MoonSharpUserData]
    [EmmyDoc("Channel defining a boolean value at any given input timing value")]
    public abstract class BooleanChannel : ISerializableUnit, IChannel
    {
        public abstract bool ValueAt(int timing);

        [MoonSharpHidden]
        public abstract List<object> SerializeProperties(ScenecontrolSerialization serialization);

        [MoonSharpHidden]
        public abstract void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization);
    }
}