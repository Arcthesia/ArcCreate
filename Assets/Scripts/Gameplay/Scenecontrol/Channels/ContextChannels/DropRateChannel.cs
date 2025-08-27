using System.Collections.Generic;
using ArcCreate.Gameplay.Chart;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class DropRateChannel : ValueChannel
    {
        private TimingGroup group;

        public DropRateChannel()
        {
            group = Services.Chart.GetTimingGroup(0);
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            int tg = (int)(long)properties[0];
            group = Services.Chart.GetTimingGroup(tg);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object> { group.GroupNumber };
        }

        public override float ValueAt(int timing)
        {
            return group.GroupProperties.DropRate > 0 ? group.GroupProperties.DropRate : Settings.DropRate.Value;
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield break;
        }
    }
}