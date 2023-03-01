using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public abstract class TextChannel : ISerializableUnit
    {
        public abstract int MaxLength { get; }

        public abstract char[] ValueAt(int timing, out int length);

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