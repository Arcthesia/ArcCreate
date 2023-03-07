using System;
using System.Collections.Generic;
using ArcCreate.Utility.Extension;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Channel whose text value is defined by interpolating between keyframes")]
    public class KeyTextChannel : TextChannel, IComparer<TextKey>
    {
        private readonly List<TextKey> keys = new List<TextKey>();
        private Func<float, float, float, float> defaultEasing;
        private string defaultEasingString;
        private bool transitionFromFirstDifference = true;
        private char[] charArray;

        public int KeyCount => keys.Count;

        public override int MaxLength => charArray.Length;

        [MoonSharpUserDataMetamethod("__concat")]
        public static ConcatTextChannel Concat(KeyTextChannel a, TextChannel b)
        {
            return new ConcatTextChannel(a, b);
        }

        [EmmyDoc("Sets the default easing to assign to keyframe for any subsequent keys added to this channel that does not have any easing defined")]
#pragma warning disable
        public KeyTextChannel SetDefaultEasing(
            [EmmyChoice(
                "linear", "l", "inconstant", "inconst", "cnsti",
                "outconstant", "outconst", "cnsto", "inoutconstant", "inoutconst",
                "cnstb", "insine", "si", "outsine", "so",
                "inoutsine", "b", "inquadratic", "inquad", "2i",
                "outquadratic", "outquad", "2o", "inoutquadratic", "inoutquad",
                "2b", "incubic", "3i", "outcubic", "outcube",
                "3o", "inoutcubic", "inoutcube", "3b", "inquartic",
                "inquart", "4i", "outquartic", "outquart", "4o",
                "inoutquartic", "inoutquart", "4b", "inquintic", "inquint",
                "5i", "outquintic", "outquint", "5o", "inoutquintic",
                "inoutquint", "5b", "inexponential", "inexpo", "exi",
                "outexponential", "outexpo", "exo", "inoutexponential", "inoutexpo",
                "exb", "incircle", "incirc", "ci", "outcircle",
                "outcirc", "co", "inoutcircle", "inoutcirc", "cb",
                "inback", "bki", "outback", "bko", "inoutback",
                "bkb", "inelastic", "eli", "outelastic", "elo",
                "inoutelastic", "elb", "inbounce", "bni", "outbounce",
                "bno", "inoutbounce", "bnb")]
            string easing)
#pragma warning restore
        {
            defaultEasing = Easing.FromString(easing);
            defaultEasingString = easing;
            return this;
        }

        [EmmyDoc("Sets subsequently added key to transition from the beginning of the text")]
        public KeyTextChannel TransitionFromStart()
        {
            transitionFromFirstDifference = false;
            return this;
        }

        [EmmyDoc("Sets subsequently added key to transition from first difference to the previous key's content")]
        public KeyTextChannel TransitionFromFirstDifference()
        {
            transitionFromFirstDifference = true;
            return this;
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

        [EmmyDoc("Add a keyframe to this channel")]
#pragma warning disable
        public KeyTextChannel AddKey(
            int timing,
            string value,
            [EmmyChoice(
                "linear", "l", "inconstant", "inconst", "cnsti",
                "outconstant", "outconst", "cnsto", "inoutconstant", "inoutconst",
                "cnstb", "insine", "si", "outsine", "so",
                "inoutsine", "b", "inquadratic", "inquad", "2i",
                "outquadratic", "outquad", "2o", "inoutquadratic", "inoutquad",
                "2b", "incubic", "3i", "outcubic", "outcube",
                "3o", "inoutcubic", "inoutcube", "3b", "inquartic",
                "inquart", "4i", "outquartic", "outquart", "4o",
                "inoutquartic", "inoutquart", "4b", "inquintic", "inquint",
                "5i", "outquintic", "outquint", "5o", "inoutquintic",
                "inoutquint", "5b", "inexponential", "inexpo", "exi",
                "outexponential", "outexpo", "exo", "inoutexponential", "inoutexpo",
                "exb", "incircle", "incirc", "ci", "outcircle",
                "outcirc", "co", "inoutcircle", "inoutcirc", "cb",
                "inback", "bki", "outback", "bko", "inoutback",
                "bkb", "inelastic", "eli", "outelastic", "elo",
                "inoutelastic", "elb", "inbounce", "bni", "outbounce",
                "bno", "inoutbounce", "bnb")]
            string easing = null)
#pragma warning restore
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
                Value = value?.ToCharArray() ?? new char[0],
                EasingString = estr,
                Easing = e,
                OverrideIndex = overrideIndex,
                TransitionFrom = transitionFrom,
            });

            keys.Sort(this);
            EnsureArraySize(value?.Length ?? 0);
            return this;
        }

        [EmmyDoc("Remove the first key that has matching timing value")]
        public KeyTextChannel RemoveKeyAtTiming(int timing)
        {
            int index = GetKeyIndex(timing);
            if (keys[index].Timing == timing)
            {
                keys.RemoveAt(index);
            }

            return this;
        }

        [MoonSharpHidden]
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
                EnsureArraySize(key?.Value?.Length ?? 0);
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