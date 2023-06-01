using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class AbsChannel : ValueChannel
    {
        private ValueChannel target;

        public AbsChannel()
        {
        }

        public AbsChannel(ValueChannel channel)
        {
            target = channel;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            target = deserialization.GetUnitFromId<ValueChannel>(properties[0]);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                serialization.AddUnitAndGetId(target),
            };
        }

        public override float ValueAt(int timing)
        {
            return Mathf.Abs(target.ValueAt(timing));
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield return target;
        }
    }
}