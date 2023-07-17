using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class BooleanConstantChannel : BooleanChannel
    {
        private bool value;

        public BooleanConstantChannel()
        {
        }

        public BooleanConstantChannel(bool value)
        {
            this.value = value;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            value = (bool)properties[0];
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>()
            {
                value,
            };
        }

        public override bool ValueAt(int timing)
            => value;
    }
}