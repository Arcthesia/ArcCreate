using System.Collections.Generic;
using System.Text;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Channel whose text value is unchanged")]
    public class ConstantTextChannel : TextChannel
    {
        private char[] charArray;
        private bool readOnce = false;

        public ConstantTextChannel()
        {
        }

        public ConstantTextChannel(string str)
        {
            charArray = str?.ToCharArray() ?? new char[0];
        }

        public override int MaxLength => charArray.Length;

        [MoonSharpUserDataMetamethod("__concat")]
        public static ConcatTextChannel Concat(ConstantTextChannel a, TextChannel b)
        {
            return new ConcatTextChannel(a, b);
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            charArray = ((string)properties[0]).ToCharArray();
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < charArray.Length; i++)
            {
                str.Append(charArray[i]);
            }

            return new List<object> { str.ToString() };
        }

        public override char[] ValueAt(int timing, out int length, out bool hasChanged)
        {
            hasChanged = !readOnce;
            readOnce = true;
            length = charArray.Length;
            return charArray;
        }
    }
}