using System.Collections.Generic;
using MoonSharp.Interpreter;
using Random = System.Random;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class RandomChannel : ValueChannel
    {
        private Random randomGenerator;
        private int seed;
        private ValueChannel min;
        private ValueChannel max;

        public RandomChannel()
        {
        }

        public RandomChannel(int seed, ValueChannel min, ValueChannel max)
        {
            randomGenerator = new Random(seed);
            this.seed = seed;
            this.min = min;
            this.max = max;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            seed = (int)properties[0];
            randomGenerator = new Random(seed);
            min = deserialization.GetUnitFromId((int)properties[1]) as ValueChannel;
            max = deserialization.GetUnitFromId((int)properties[2]) as ValueChannel;
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                seed,
                serialization.AddUnitAndGetId(min),
                serialization.AddUnitAndGetId(max),
            };
        }

        public override float ValueAt(int timing)
        {
            float minVal = min.ValueAt(timing);
            float maxVal = max.ValueAt(timing);
            return minVal + ((maxVal - minVal) * (float)randomGenerator.NextDouble());
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield return min;
            yield return max;
        }
    }
}