using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class ClampChannel : ValueChannel
    {
        private ValueChannel a;

        private ValueChannel b;

        private ValueChannel c;

        public ClampChannel()
        {
        }

        public ClampChannel(ValueChannel a, ValueChannel b, ValueChannel c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            a = deserialization.GetUnitFromId((int)properties[0]) as ValueChannel;
            b = deserialization.GetUnitFromId((int)properties[1]) as ValueChannel;
            c = deserialization.GetUnitFromId((int)properties[2]) as ValueChannel;
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                serialization.AddUnitAndGetId(a),
                serialization.AddUnitAndGetId(b),
                serialization.AddUnitAndGetId(c),
            };
        }

        public override float ValueAt(int timing)
        {
            return Mathf.Clamp(a.ValueAt(timing), b.ValueAt(timing), c.ValueAt(timing));
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield return a;
            yield return b;
            yield return c;
        }
    }
}