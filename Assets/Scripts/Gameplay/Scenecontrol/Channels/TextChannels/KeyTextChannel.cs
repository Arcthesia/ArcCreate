using System;
using System.Collections.Generic;
using ArcCreate.Utility.Extension;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class KeyTextChannel : TextChannel, IComparer<TextKey>
    {
        private readonly List<TextKey> keys = new List<TextKey>();
        private Func<float, float, float, float> defaultEasing;
        private string defaultEasingString;
        private bool transitionFromFirstDifference = true;
        private char[] charArray;

        public int KeyCount => keys.Count;

        public override int MaxLength => charArray.Length;

        public TextChannel SetDefaultEasing(string easing)
        {
            defaultEasing = Easing.FromString(easing);
            defaultEasingString = easing;
            return this;
        }

        public KeyTextChannel TransitionFromStart()
        {
            transitionFromFirstDifference = false;
            return this;
        }

        public KeyTextChannel TransitionFromFirstDifference()
        {
            transitionFromFirstDifference = true;
            return this;
        }

        [MoonSharpUserDataMetamethod("__concat")]
        public ConcatTextChannel Concat(TextChannel channel)
        {
            return new ConcatTextChannel(this, channel);
        }

        public override char[] ValueAt(int timing, out int length)
        {
            if (keys.Count == 0)
            {
                length = 0;
                return charArray;
            }

            if (keys.Count == 1 || timing <= keys[0].Timing)
            {
                length = keys[0].Value.Length;
                return keys[0].Value;
            }

            if (timing >= keys[keys.Count - 1].Timing)
            {
                length = keys[keys.Count - 1].Value.Length;
                return keys[keys.Count - 1].Value;
            }

            int index = GetKeyIndex(timing);
            int timing1 = keys[index].Timing;
            int timing2 = keys[index + 1].Timing;
            TextKey key1 = keys[index];
            TextKey key2 = keys[index + 1];

            if (timing1 == timing2)
            {
                TextKey outp = key1.OverrideIndex > key2.OverrideIndex ? key1 : key2;
                length = outp.Value.Length;
                return outp.Value;
            }

            float p = (float)(timing - timing1) / (timing2 - timing1);
            int key1Factor = key1.Value.Length - key2.TransitionFrom;
            int key2Factor = key2.Value.Length - key2.TransitionFrom;
            int stringBlend = UnityEngine.Mathf.RoundToInt(key1.Easing(key1Factor, -key2Factor, p));

            Array.Copy(key1.Value, 0, charArray, 0, key2.TransitionFrom);
            if (stringBlend > 0)
            {
                Array.Copy(key1.Value, key2.TransitionFrom, charArray, key2.TransitionFrom, stringBlend);
                length = key2.TransitionFrom + stringBlend;
            }
            else
            {
                Array.Copy(key2.Value, key2.TransitionFrom, charArray, key2.TransitionFrom, -stringBlend);
                length = key2.TransitionFrom - stringBlend;
            }

            return charArray;
        }

        public KeyTextChannel AddKey(int timing, string value, string easing = null)
        {
            Func<float, float, float, float> e;
            string estr;
            if (easing == null)
            {
                e = defaultEasing ?? Easing.Linear;
                estr = defaultEasingString ?? "s";
            }
            else
            {
                e = Easing.FromString(easing);
                estr = easing;
            }

            int overrideIndex = 0;
            if (keys.Count > 0 && keys[GetKeyIndex(timing)].Timing == timing)
            {
                overrideIndex += 1;
            }

            int transitionFrom = 0;
            if (transitionFromFirstDifference && keys.Count > 0)
            {
                char[] prev = keys[keys.Count - 1].Value;
                while (transitionFrom < value.Length && transitionFrom < prev.Length
                    && value[transitionFrom] == prev[transitionFrom])
                {
                    transitionFrom += 1;
                }
            }

            keys.Add(new TextKey
            {
                Timing = timing,
                Value = value.ToCharArray(),
                EasingString = estr,
                Easing = e,
                OverrideIndex = overrideIndex,
                TransitionFrom = transitionFrom,
            });

            keys.Sort(this);
            EnsureArraySize(value.Length);
            return this;
        }

        public KeyTextChannel RemoveKeyAtTiming(int timing)
        {
            int index = GetKeyIndex(timing);
            if (keys[index].Timing == timing)
            {
                keys.RemoveAt(index);
            }

            return this;
        }

        public KeyTextChannel RemoveKeyAtIndex(int index)
        {
            index -= 1;
            if (index >= 0 && index < keys.Count)
            {
                keys.RemoveAt(index);
            }

            return this;
        }

        public int Compare(TextKey x, TextKey y)
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
                TextKey key = new TextKey();
                key.Deserialize(str);
                key.Easing = Easing.FromString(key.EasingString);
                keys.Add(key);
            }
        }

        private int GetKeyIndex(int timing)
        {
            return keys.BinarySearchNearest(timing, (key) => key.Timing);
        }

        private void EnsureArraySize(int length)
        {
            if (charArray == null)
            {
                charArray = new char[length];
                return;
            }

            if (length > charArray.Length)
            {
                Array.Resize(ref charArray, length);
            }
        }
    }
}