using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class ConditionalChannel : ValueChannel
    {
        private ValueChannel control;
        private ValueChannel threshold;
        private ValueChannel ifAbove;
        private ValueChannel ifEqual;
        private ValueChannel ifBelow;

        public ConditionalChannel()
        {
        }

        public ConditionalChannel(ValueChannel control, ValueChannel threshold, ValueChannel ifAbove, ValueChannel ifEqual, ValueChannel ifBelow)
        {
            this.control = control;
            this.threshold = threshold;
            this.ifAbove = ifAbove;
            this.ifEqual = ifEqual;
            this.ifBelow = ifBelow;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            control = deserialization.GetUnitFromId<ValueChannel>(properties[0]);
            threshold = deserialization.GetUnitFromId<ValueChannel>(properties[1]);
            ifAbove = deserialization.GetUnitFromId<ValueChannel>(properties[2]);
            ifEqual = deserialization.GetUnitFromId<ValueChannel>(properties[3]);
            ifBelow = deserialization.GetUnitFromId<ValueChannel>(properties[4]);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                serialization.AddUnitAndGetId(control),
                serialization.AddUnitAndGetId(threshold),
                serialization.AddUnitAndGetId(ifAbove),
                serialization.AddUnitAndGetId(ifEqual),
                serialization.AddUnitAndGetId(ifBelow),
            };
        }

        public override float ValueAt(int timing)
        {
            int comp = control.ValueAt(timing).CompareTo(threshold.ValueAt(timing));
            if (comp > 0)
            {
                return ifAbove.ValueAt(timing);
            }
            else if (comp < 0)
            {
                return ifBelow.ValueAt(timing);
            }

            return ifEqual.ValueAt(timing);
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield return control;
            yield return threshold;
            yield return ifAbove;
            yield return ifEqual;
            yield return ifBelow;
        }
    }
}