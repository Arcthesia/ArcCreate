using System.Collections.Generic;
using ArcCreate.Gameplay.Chart;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class BPMChannel : ValueChannel
    {
        private TimingGroup group;

        public BPMChannel()
        {
        }

        public BPMChannel(int tg)
        {
            group = Services.Chart.GetTimingGroup(tg);
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            int tg = (int)properties[0];
            group = Services.Chart.GetTimingGroup(tg);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object> { group.GroupNumber };
        }

        public override float ValueAt(int timing)
        {
            return group.GetBpm(timing);
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield break;
        }
    }
}