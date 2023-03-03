using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public abstract class ValueChannel : ISerializableUnit
    {
        public static ValueChannel ConstantZeroChannel { get; } = new ConstantChannel(0);

        public static ValueChannel ConstantOneChannel { get; } = new ConstantChannel(1);

        public string Name { get; set; }

        public static ValueChannel operator +(ValueChannel a) => a;

        public static NegateChannel operator -(ValueChannel a) => new NegateChannel(a);

        public static SumChannel operator +(ValueChannel a, ValueChannel b) => new SumChannel(a, b);

        public static SumChannel operator +(ValueChannel a, float b) => a + new ConstantChannel(b);

        public static SumChannel operator +(float b, ValueChannel a) => a + new ConstantChannel(b);

        public static SumChannel operator -(ValueChannel a, ValueChannel b) => new SumChannel(a, -b);

        public static SumChannel operator -(ValueChannel a, float b) => a + new ConstantChannel(-b);

        public static SumChannel operator -(float b, ValueChannel a) => -a + new ConstantChannel(b);

        public static ProductChannel operator *(ValueChannel a, float b) => a * new ConstantChannel(b);

        public static ProductChannel operator *(float b, ValueChannel a) => a * new ConstantChannel(b);

        public static ProductChannel operator *(ValueChannel a, ValueChannel b) => new ProductChannel(a, b);

        public static ProductChannel operator /(ValueChannel a, float b) => a * new ConstantChannel(1 / b);

        public static ProductChannel operator /(float b, ValueChannel a) => new ConstantChannel(b) * new InverseChannel(a);

        public static ProductChannel operator /(ValueChannel a, ValueChannel b) => a * new InverseChannel(b);

        public abstract float ValueAt(int timing);

        public virtual void Reset()
        {
        }

        public virtual void Destroy()
        {
        }

        public abstract List<object> SerializeProperties(ScenecontrolSerialization serialization);

        public abstract void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization);

        public ValueChannel Find(string name)
        {
            List<object> properties = SerializeProperties(new ScenecontrolSerialization());
            foreach (var prop in properties)
            {
                if (prop is ValueChannel channel
                 && channel.Name == name)
                {
                    return channel;
                }
            }

            return null;
        }
    }
}