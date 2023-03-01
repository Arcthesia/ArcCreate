using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class ConcatTextChannel : TextChannel
    {
        private List<TextChannel> components;
        private char[] charArray = new char[0];

        public ConcatTextChannel()
        {
        }

        public ConcatTextChannel(TextChannel a, TextChannel b)
        {
            components = new List<TextChannel>() { a, b };
            int newLength = a.MaxLength + b.MaxLength;
            if (newLength > charArray.Length)
            {
                Array.Resize(ref charArray, newLength);
            }
        }

        public override int MaxLength => charArray.Length;

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            components = new List<TextChannel>();
            foreach (var prop in properties)
            {
                components.Add(deserialization.GetUnitFromId((int)prop) as TextChannel);
            }
        }

        [MoonSharpUserDataMetamethod("__concat")]
        public ConcatTextChannel Concat(TextChannel channel)
        {
            components.Add(channel);
            int size = 0;
            foreach (var c in components)
            {
                size += c.MaxLength;
            }

            if (size > charArray.Length)
            {
                Array.Resize(ref charArray, size);
            }

            return this;
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            List<object> result = new List<object>();
            foreach (var comp in components)
            {
                result.Add(serialization.AddUnitAndGetId(comp));
            }

            return result;
        }

        public override char[] ValueAt(int timing, out int length)
        {
            length = 0;
            for (int i = 0; i < components.Count; i++)
            {
                TextChannel c = components[i];
                char[] partial = c.ValueAt(timing, out int partialLength);
                Array.Copy(partial, 0, charArray, length, partialLength);
                length += partialLength;
            }

            return charArray;
        }
    }
}