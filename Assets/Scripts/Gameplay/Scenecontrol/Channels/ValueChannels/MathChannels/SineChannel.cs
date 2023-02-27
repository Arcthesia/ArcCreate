using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class SineChannel : ValueChannel
    {
        private ValueChannel period;
        private ValueChannel offset;
        private ValueChannel min;
        private ValueChannel max;

        public SineChannel()
        {
        }

        public SineChannel(ValueChannel period, ValueChannel min, ValueChannel max, ValueChannel offset)
        {
            this.period = period;
            this.min = min;
            this.max = max;
            this.offset = offset;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            period = deserialization.GetUnitFromId((int)properties[0]) as ValueChannel;
            offset = deserialization.GetUnitFromId((int)properties[1]) as ValueChannel;
            min = deserialization.GetUnitFromId((int)properties[2]) as ValueChannel;
            max = deserialization.GetUnitFromId((int)properties[3]) as ValueChannel;
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                serialization.AddUnitAndGetId(period),
                serialization.AddUnitAndGetId(offset),
                serialization.AddUnitAndGetId(min),
                serialization.AddUnitAndGetId(max),
            };
        }

        public override float ValueAt(int timing)
        {
            float omega = 2 * Mathf.PI / (float)period.ValueAt(timing);
            float minVal = min.ValueAt(timing);
            float maxVal = max.ValueAt(timing);
            return minVal + ((maxVal - minVal) * Mathf.Sin(omega * (timing + offset.ValueAt(timing))));
        }
    }
}