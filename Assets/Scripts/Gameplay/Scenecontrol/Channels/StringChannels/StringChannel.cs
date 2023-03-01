using System.Collections.Generic;
using ArcCreate.Utility.Extension;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public abstract class StringChannel : ISerializableUnit
    {
        public abstract string ValueAt(int timing);

        public void Reset()
        {
        }

        public void Destroy()
        {
        }

        public abstract List<object> SerializeProperties(ScenecontrolSerialization serialization);

        public abstract void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization);
    }
}