using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class TimeShiftChannel : ValueChannel
    {
        private ValueChannel value;
        private ValueChannel shift;

        public TimeShiftChannel()
        {
        }

        public TimeShiftChannel(ValueChannel a, ValueChannel b)
        {
            this.value = a;
            this.shift = b;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            value = deserialization.GetUnitFromId<ValueChannel>(properties[0]);
            shift = deserialization.GetUnitFromId<ValueChannel>(properties[1]);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                serialization.AddUnitAndGetId(value),
                serialization.AddUnitAndGetId(shift),
            };
        }

        public override float ValueAt(int timing)
            => value.ValueAt(timing + (int)shift.ValueAt(timing));

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield return value;
            yield return shift;
        }
    }
}