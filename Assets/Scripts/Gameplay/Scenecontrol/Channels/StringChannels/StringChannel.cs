using System.Collections.Generic;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Channel defining a string value at any given input timing value")]
    public abstract class StringChannel : ISerializableUnit
    {
        public abstract string ValueAt(int timing);

        [MoonSharpHidden]
        public abstract List<object> SerializeProperties(ScenecontrolSerialization serialization);

        [MoonSharpHidden]
        public abstract void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization);

        void ISerializableUnit.DeserializeProperties(List<object> properties, EnabledFeatures features, ScenecontrolDeserialization deserialization)
            => DeserializeProperties(properties, deserialization);
    }
}