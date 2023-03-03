using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class MinChannel : ValueChannel
    {
        private ValueChannel a;
        private ValueChannel b;

        public MinChannel()
        {
        }

        public MinChannel(ValueChannel a, ValueChannel b)
        {
            this.a = a;
            this.b = b;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            a = deserialization.GetUnitFromId((int)properties[0]) as ValueChannel;
            b = deserialization.GetUnitFromId((int)properties[1]) as ValueChannel;
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                serialization.AddUnitAndGetId(a),
                serialization.AddUnitAndGetId(b),
            };
        }

        public override float ValueAt(int timing)
        {
            return Mathf.Min(a.ValueAt(timing), b.ValueAt(timing));
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield return a;
            yield return b;
        }
    }
}