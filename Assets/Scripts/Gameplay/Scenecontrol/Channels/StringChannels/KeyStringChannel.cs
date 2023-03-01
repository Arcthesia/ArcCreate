using System.Collections.Generic;
using ArcCreate.Utility.Extension;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class KeyStringChannel : StringChannel, IComparer<StringKey>
    {
        private readonly List<StringKey> keys = new List<StringKey>();

        public int KeyCount => keys.Count;

        public override string ValueAt(int timing)
        {
            if (keys.Count == 0)
            {
                return "";
            }

            if (keys.Count == 1)
            {
                return keys[0].Value;
            }

            if (timing <= keys[0].Timing)
            {
                return keys[0].Value;
            }

            if (timing >= keys[keys.Count - 1].Timing)
            {
                return keys[keys.Count - 1].Value;
            }

            int index = GetKeyIndex(timing);
            return keys[index].Value;
        }

        public StringChannel AddKey(int timing, string value)
        {
            int overrideIndex = 0;
            if (keys.Count > 0 && keys[GetKeyIndex(timing)].Timing == timing)
            {
                overrideIndex += 1;
            }

            keys.Add(new StringKey
            {
                Timing = timing,
                Value = value,
                OverrideIndex = overrideIndex,
            });

            keys.Sort(this);
            return this;
        }

        public StringChannel RemoveKeyAtTiming(int timing)
        {
            int index = GetKeyIndex(timing);
            if (keys[index].Timing == timing)
            {
                keys.RemoveAt(index);
            }

            return this;
        }

        public StringChannel RemoveKeyAtIndex(int index)
        {
            index -= 1;
            if (index >= 0 && index < keys.Count)
            {
                keys.RemoveAt(index);
            }

            return this;
        }

        public int Compare(StringKey x, StringKey y)
        {
            if (x.Timing == y.Timing)
            {
                return x.OverrideIndex.CompareTo(y.OverrideIndex);
            }

            return x.Timing.CompareTo(y.Timing);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            List<object> result = new List<object>(keys.Count);

            foreach (var key in keys)
            {
                result.Add(key.Serialize());
            }

            return result;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            keys.Clear();
            for (int i = 0; i < properties.Count; i++)
            {
                object obj = properties[i];
                string str = obj as string;
                StringKey key = new StringKey();
                key.Deserialize(str);
                keys.Add(key);
            }
        }

        private int GetKeyIndex(int timing)
        {
            return keys.BisectLeft(timing, (key) => key.Timing);
        }
    }
}