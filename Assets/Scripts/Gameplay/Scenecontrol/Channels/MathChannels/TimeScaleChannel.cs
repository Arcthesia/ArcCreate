using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class TimeScaleChannel : ValueChannel
    {
        private ValueChannel value;
        private ValueChannel scale;

        public TimeScaleChannel()
        {
        }

        public TimeScaleChannel(ValueChannel a, ValueChannel b)
        {
            this.value = a;
            this.scale = b;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            value = deserialization.GetUnitFromId<ValueChannel>(properties[0]);
            scale = deserialization.GetUnitFromId<ValueChannel>(properties[1]);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                serialization.AddUnitAndGetId(value),
                serialization.AddUnitAndGetId(scale),
            };
        }

        public override float ValueAt(int timing)
            => value.ValueAt(timing * (int)scale.ValueAt(timing));

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield return value;
            yield return scale;
        }
    }
}