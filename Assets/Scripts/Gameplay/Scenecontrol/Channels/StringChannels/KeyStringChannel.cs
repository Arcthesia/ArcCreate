using System.Collections.Generic;
using ArcCreate.Utility.Extension;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Channel whose string value is defined by keyframes")]
    public class KeyStringChannel : StringChannel, IComparer<StringKey>
    {
        private readonly List<StringKey> keys;
        private readonly CachedBinarySearch<StringKey, int> keySearch;

        public KeyStringChannel()
        {
            keySearch = new CachedBinarySearch<StringKey, int>(new List<StringKey>(), k => k.Timing, this);
            keys = keySearch.List;
        }

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

            int index = keySearch.Search(timing);
            return keys[index].Value;
        }

        [EmmyDoc("Add a keyframe to this channel")]
        public KeyStringChannel AddKey(int timing, string value)
        {
            int overrideIndex = 0;
            if (keys.Count > 0 && keys[keySearch.Search(timing)].Timing == timing)
            {
                overrideIndex += 1;
            }

            keys.Add(new StringKey
            {
                Timing = timing,
                Value = value,
                OverrideIndex = overrideIndex,
            });

            keySearch.Sort();
            return this;
        }

        [EmmyDoc("Remove the first key that has matching timing value")]
        public KeyStringChannel RemoveKeyAtTiming(int timing)
        {
            int index = keySearch.Search(timing);
            if (keys[index].Timing == timing)
            {
                keys.RemoveAt(index);
            }

            keySearch.Sort();

            return this;
        }

        [MoonSharpHidden]
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

            keySearch.Sort();
        }
    }
}