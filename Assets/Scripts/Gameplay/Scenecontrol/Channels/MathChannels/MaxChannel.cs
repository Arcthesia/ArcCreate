using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class MaxChannel : ValueChannel
    {
        private ValueChannel a;
        private ValueChannel b;

        public MaxChannel()
        {
        }

        public MaxChannel(ValueChannel a, ValueChannel b)
        {
            this.a = a;
            this.b = b;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            a = deserialization.GetUnitFromId<ValueChannel>(properties[0]);
            b = deserialization.GetUnitFromId<ValueChannel>(properties[1]);
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
            return Mathf.Max(a.ValueAt(timing), b.ValueAt(timing));
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield return a;
            yield return b;
        }
    }
}