using System.Collections.Generic;

namespace ArcCreate.Gameplay.Scenecontrol
{
    internal class ScenecontrolVersioning : ISerializableUnit
    {
        private EnabledFeatures features;

        public EnabledFeatures Features => features;

        public ScenecontrolVersioning(EnabledFeatures features)
        {
            this.features = features;
        }

        public void DeserializeProperties(List<object> properties, EnabledFeatures features, ScenecontrolDeserialization deserialization)
        {
            this.features = (EnabledFeatures)(long)properties[0];
        }

        public List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object> { (long)features };
        }
    }
}