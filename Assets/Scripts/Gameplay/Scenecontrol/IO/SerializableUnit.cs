using System.Collections.Generic;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public abstract class SerializableUnit
    {
        public virtual void Reset()
        {
        }

        public virtual void Destroy()
        {
        }

        public abstract List<object> SerializeProperties(ScenecontrolSerialization serialization);

        public abstract void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization);
    }
}