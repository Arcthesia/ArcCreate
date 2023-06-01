using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class PureCosChannel : ValueChannel
    {
        private ValueChannel input;

        public PureCosChannel()
        {
        }

        public PureCosChannel(ValueChannel input)
        {
            this.input = input;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            input = deserialization.GetUnitFromId<ValueChannel>(properties[0]);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>()
            {
                serialization.AddUnitAndGetId(input),
            };
        }

        public override float ValueAt(int timing)
        {
            return Mathf.Cos(input.ValueAt(timing));
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield return input;
        }
    }
}