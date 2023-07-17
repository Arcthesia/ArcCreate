using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
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
            a = deserialization.GetUnitFromId<ValueChannel>(properties[0]);
            b = deserialization.GetUnitFromId<ValueChannel>(properties[1]);
            c = deserialization.GetUnitFromId<ValueChannel>(properties[2]);
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