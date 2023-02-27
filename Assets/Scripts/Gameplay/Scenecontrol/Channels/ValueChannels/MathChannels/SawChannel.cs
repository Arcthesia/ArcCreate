using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class SawChannel : ValueChannel
    {
        private ValueChannel period;
        private ValueChannel offset;
        private ValueChannel min;
        private ValueChannel max;
        private Func<float, float, float, float> easingFunc;
        private string easing;

        public SawChannel()
        {
        }

        public SawChannel(string easing, ValueChannel period, ValueChannel min, ValueChannel max, ValueChannel offset)
        {
            this.easing = easing;
            this.easingFunc = Easing.FromString(easing);
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
            easing = (string)properties[4];
            easingFunc = Easing.FromString(easing);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                serialization.AddUnitAndGetId(period),
                serialization.AddUnitAndGetId(offset),
                serialization.AddUnitAndGetId(min),
                serialization.AddUnitAndGetId(max),
                easing,
            };
        }

        public override float ValueAt(int timing)
        {
            float looped = (timing + offset.ValueAt(timing)) % period.ValueAt(timing);
            return easingFunc(min.ValueAt(timing), max.ValueAt(timing), (float)looped / period.ValueAt(timing));
        }
    }
}