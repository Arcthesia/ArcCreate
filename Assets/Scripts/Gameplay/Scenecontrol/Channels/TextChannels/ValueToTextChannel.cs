using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class ValueToTextChannel : TextChannel
    {
        private ValueChannel source;
        private char[] charArray;
        private int precision;

        public ValueToTextChannel()
        {
        }

        public ValueToTextChannel(ValueChannel source, int maxLength = 10, int precision = 0)
        {
            this.source = source;
            this.charArray = new char[maxLength];
            this.precision = precision;
        }

        public override int MaxLength => charArray.Length;

        [MoonSharpUserDataMetamethod("__concat")]
        public static ConcatTextChannel Concat(ValueToTextChannel a, TextChannel b)
        {
            return new ConcatTextChannel(a, b);
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            source = deserialization.GetUnitFromId<ValueChannel>(properties[0]);
            charArray = new char[(int)(double)properties[1]];
            precision = (int)(double)properties[2];
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            List<object> result = new List<object>
            {
                serialization.AddUnitAndGetId(source),
                charArray.Length,
                precision,
            };

            return result;
        }

        public override char[] ValueAt(int timing, out int length)
        {
            float number = source.ValueAt(timing);
            bool isNegativeNumber = number < 0f;
            number = isNegativeNumber ? -number : number;

            int integer = (int)number;
            double fraction = (double)number - integer;
            float epsilon = Mathf.Pow(10, -precision);

            int charIndex = precision;
            int truncateAmount = 0;
            for (int i = 0; i < precision; i++)
            {
                // Floating point arithmetics are not deterministic, try to handle
                // edge cases properly via these if conditions
                if (fraction >= 0f)
                {
                    fraction *= 10f;
                }
                else
                {
                    fraction *= -10f;
                }

                int digit;
                double diff = Math.Abs(fraction - Math.Round(fraction));
                if (Math.Abs(fraction - Math.Round(fraction)) <= epsilon)
                {
                    digit = Mathf.RoundToInt((float)fraction);
                }
                else
                {
                    digit = (int)fraction;
                }

                if (digit <= 0)
                {
                    digit = 0;
                    truncateAmount++;
                }
                else
                {
                    if (digit > 9)
                    {
                        digit = 9;
                    }

                    truncateAmount = 0;
                }

                charArray[--charIndex] = (char)('0' + digit);
                fraction -= digit;
            }

            // Assert: if truncateAmount == precision, then fraction consists of all 0's,
            // so don't include the fraction in the text at all
            if (truncateAmount < precision)
            {
                if (truncateAmount > 0)
                {
                    // Truncate redundant 0's
                    for (int i = charIndex + truncateAmount, j = i + precision; i < j; i++)
                    {
                        charArray[i - truncateAmount] = charArray[i];
                    }
                }

                charIndex += precision - truncateAmount;
                charArray[charIndex++] = '.';
            }

            // Rare case: -0.0001 with precision 2 results in -0, this line fixes it
            else if (integer == 0)
            {
                isNegativeNumber = false;
            }

            if (truncateAmount == precision || precision == 0)
            {
                integer = UnityEngine.Mathf.RoundToInt(number);
            }

            if (integer == 0)
            {
                charArray[charIndex++] = '0';
            }
            else
            {
                while (integer > 0)
                {
                    charArray[charIndex++] = (char)('0' + (integer % 10));
                    integer /= 10;
                }
            }

            if (isNegativeNumber)
            {
                charArray[charIndex++] = '-';
            }

            // Reverse array
            for (int i = (charIndex / 2) - 1; i >= 0; i--)
            {
                int j = charIndex - 1 - i;

                (charArray[j], charArray[i]) = (charArray[i], charArray[j]);
            }

            length = charIndex;
            return charArray;
        }
    }
}