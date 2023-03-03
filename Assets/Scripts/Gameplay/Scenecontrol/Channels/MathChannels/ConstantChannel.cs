using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class ConstantChannel : ValueChannel
    {
        private float val;

        public ConstantChannel()
        {
        }

        public ConstantChannel(float val)
        {
            this.val = val;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            val = (float)properties[0];
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object> { val };
        }

        public override float ValueAt(int timing) => val;

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield break;
        }
    }
}