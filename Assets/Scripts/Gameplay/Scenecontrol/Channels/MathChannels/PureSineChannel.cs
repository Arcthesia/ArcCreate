using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class PureSineChannel : ValueChannel
    {
        private ValueChannel input;

        public PureSineChannel()
        {
        }

        public PureSineChannel(ValueChannel input)
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
            return Mathf.Sin(input.ValueAt(timing));
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield return input;
        }
    }
}