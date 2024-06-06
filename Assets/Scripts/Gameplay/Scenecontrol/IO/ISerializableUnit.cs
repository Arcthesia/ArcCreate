using System.Collections.Generic;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public interface ISerializableUnit
    {
        List<object> SerializeProperties(ScenecontrolSerialization serialization);

        void DeserializeProperties(List<object> properties, EnabledFeatures features, ScenecontrolDeserialization deserialization);
    }
}