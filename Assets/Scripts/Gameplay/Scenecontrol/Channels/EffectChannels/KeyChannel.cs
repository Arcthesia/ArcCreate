using System;
using System.Collections.Generic;
using ArcCreate.Utility.Extension;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Channel whose value is defined by interpolating between keyframes")]
    public class KeyChannel : ValueChannel, IComparer<Key>
    {
        private readonly List<Key> keys = new List<Key>();
        private Func<float, float, float, float> defaultEasing;
        private string defaultEasingString;

        public bool IntroExtrapolation { get; set; } = false;

        public bool OuttroExtrapolation { get; set; } = false;

        public int KeyCount => keys.Count;

        [EmmyDoc("Sets the default easing to assign to keyframe for any subsequent keys added to this channel that does not have any easing defined")]
#pragma warning disable
        public KeyChannel SetDefaultEasing(
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

        [EmmyDoc("Sets whether or not to extrapolate value for timing values before the first key")]
        public KeyChannel SetIntroExtrapolation(bool extrapolation)
        {
            IntroExtrapolation = extrapolation;
            return this;
        }

        [EmmyDoc("Sets whether or not to extrapolate value for timing values after the last key")]
        public KeyChannel SetOuttroExtrapolation(bool extrapolation)
        {
            OuttroExtrapolation = extrapolation;
            return this;
        }

        public override float ValueAt(int timing)
        {
            if (keys.Count == 0)
            {
                return 0;
            }

            if (keys.Count == 1)
            {
                return keys[0].Value;
            }

            // Extrapolate
            if (timing <= keys[0].Timing)
            {
                if (IntroExtrapolation)
                {
                    float extrapolatedP = (float)(timing - keys[0].Timing) / (keys[1].Timing - keys[0].Timing);
                    return keys[0].Easing(keys[0].Value, keys[1].Value, extrapolatedP);
                }
                else
                {
                    return keys[0].Value;
                }
            }

            if (timing >= keys[keys.Count - 1].Timing)
            {
                if (OuttroExtrapolation)
                {
                    float extrapolatedP = (float)(timing - keys[keys.Count - 2].Timing) / (keys[keys.Count - 1].Timing - keys[keys.Count - 2].Timing);
                    return keys[keys.Count - 2].Easing(keys[keys.Count - 2].Value, keys[keys.Count - 1].Value, extrapolatedP);
                }
                else
                {
                    return keys[keys.Count - 1].Value;
                }
            }

            int index = GetKeyIndex(timing);
            int timing1 = keys[index].Timing;
            int timing2 = keys[index + 1].Timing;
            Key key1 = keys[index];
            Key key2 = keys[index + 1];

            if (timing1 == timing2)
            {
                return key1.OverrideIndex > key2.OverrideIndex ? key1.Value : key2.Value;
            }

            float p = (float)(timing - timing1) / (timing2 - timing1);
            float value = (float)key1.Easing(key1.Value, key2.Value, p);

            return value;
        }

        [EmmyDoc("Add a keyframe to this channel")]
#pragma warning disable
        public KeyChannel AddKey(
            int timing,
            float value,
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

            Key key = new Key
            {
                Timing = timing,
                Value = value,
                Easing = e,
                EasingString = estr,
                OverrideIndex = overrideIndex,
            };

            keys.Add(key);
            keys.Sort(this);
            return this;
        }

        [EmmyDoc("Remove the first key that has matching timing value")]
        public KeyChannel RemoveKeyAtTiming(int timing)
        {
            int index = GetKeyIndex(timing);
            if (keys[index].Timing == timing)
            {
                keys.RemoveAt(index);
            }

            return this;
        }

        [MoonSharpHidden]
        public int Compare(Key x, Key y)
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
                Key key = new Key();
                key.Deserialize(str);
                key.Easing = Easing.FromString(key.EasingString);
                keys.Add(key);
            }
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield break;
        }

        private int GetKeyIndex(int timing)
        {
            return keys.BinarySearchNearest(timing, (key) => key.Timing);
        }
    }
}