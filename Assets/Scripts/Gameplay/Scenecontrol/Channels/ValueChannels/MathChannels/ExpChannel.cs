using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class ExpChannel : ValueChannel
    {
        private ValueChannel num;
        private ValueChannel exp;

        public ExpChannel()
        {
        }

        public ExpChannel(ValueChannel num, ValueChannel exp)
        {
            this.num = num;
            this.exp = exp;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            num = deserialization.GetUnitFromId((int)properties[0]) as ValueChannel;
            exp = deserialization.GetUnitFromId((int)properties[1]) as ValueChannel;
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                serialization.AddUnitAndGetId(num),
                serialization.AddUnitAndGetId(exp),
            };
        }

        public override float ValueAt(int timing)
        {
            return Mathf.Pow(num.ValueAt(timing), exp.ValueAt(timing));
        }
    }
}