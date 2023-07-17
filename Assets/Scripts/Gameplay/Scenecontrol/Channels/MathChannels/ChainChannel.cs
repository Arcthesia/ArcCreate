using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class ChainChannel : ValueChannel
    {
        private ValueChannel outer;
        private ValueChannel inner;

        public ChainChannel()
        {
        }

        public ChainChannel(ValueChannel a, ValueChannel b)
        {
            this.outer = a;
            this.inner = b;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            outer = deserialization.GetUnitFromId<ValueChannel>(properties[0]);
            inner = deserialization.GetUnitFromId<ValueChannel>(properties[1]);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                serialization.AddUnitAndGetId(outer),
                serialization.AddUnitAndGetId(inner),
            };
        }

        public override float ValueAt(int timing)
            => outer.ValueAt((int)inner.ValueAt(timing));

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield return outer;
            yield return inner;
        }
    }
}