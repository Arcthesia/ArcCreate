using System;
using System.Collections.Generic;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Text channel that's combined from multiple other text channel")]
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
            EnsureArraySize();
        }

        public override int MaxLength => charArray.Length;

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            components = new List<TextChannel>();
            foreach (var prop in properties)
            {
                components.Add(deserialization.GetUnitFromId((int)prop) as TextChannel);
            }

            EnsureArraySize();
        }

        [MoonSharpUserDataMetamethod("__concat")]
        public ConcatTextChannel Concat(TextChannel channel)
        {
            components.Add(channel);
            EnsureArraySize();
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
            EnsureArraySize();
            for (int i = 0; i < components.Count; i++)
            {
                TextChannel c = components[i];
                char[] partial = c.ValueAt(timing, out int partialLength);
                Array.Copy(partial, 0, charArray, length, partialLength);
                length += partialLength;
            }

            return charArray;
        }

        private void EnsureArraySize()
        {
            int length = 0;
            foreach (var c in components)
            {
                length += c.MaxLength;
            }

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