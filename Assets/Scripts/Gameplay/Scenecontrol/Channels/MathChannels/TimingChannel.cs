using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class TimingChannel : ValueChannel
    {
        public TimingChannel()
        {
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>();
        }

        public override float ValueAt(int timing)
            => timing;

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield break;
        }
    }
}