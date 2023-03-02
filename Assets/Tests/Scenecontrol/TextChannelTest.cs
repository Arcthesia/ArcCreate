using System.Text;
using ArcCreate.Gameplay.Scenecontrol;
using NUnit.Framework;

namespace Tests.Unit
{
    public class TextChannelTest
    {
        [Test]
        public void StringKeyChannel_OneKey()
        {
            StringChannel channel = new KeyStringChannel()
                .AddKey(0, "str");

            Assert.That(channel.ValueAt(0), Is.EqualTo("str"));
            Assert.That(channel.ValueAt(1), Is.EqualTo("str"));
            Assert.That(channel.ValueAt(-1), Is.EqualTo("str"));
        }

        [Test]
        public void StringKeyChannel_MultipleKey()
        {
            StringChannel channel = new KeyStringChannel()
                .AddKey(0, "str")
                .AddKey(2, "str2")
                .AddKey(4, "str3");

            Assert.That(channel.ValueAt(0), Is.EqualTo("str"));
            Assert.That(channel.ValueAt(1), Is.EqualTo("str"));
            Assert.That(channel.ValueAt(2), Is.EqualTo("str2"));
            Assert.That(channel.ValueAt(3), Is.EqualTo("str2"));
            Assert.That(channel.ValueAt(4), Is.EqualTo("str3"));
            Assert.That(channel.ValueAt(5), Is.EqualTo("str3"));
        }

        [Test]
        public void StringKeyChannel_MultipleKeyOverlapping()
        {
            StringChannel channel = new KeyStringChannel()
                .AddKey(0, "str")
                .AddKey(0, "str2")
                .AddKey(0, "str3")
                .AddKey(2, "str4");

            Assert.That(channel.ValueAt(1), Is.EqualTo("str3"));
        }

        [Test]
        public void TextKeyChannel_OneKey()
        {
            TextChannel channel = new KeyTextChannel()
                .AddKey(0, "str");

            AssertCharArrayEquality(channel, 0, "str");
            AssertCharArrayEquality(channel, -1, "str");
            AssertCharArrayEquality(channel, 1, "str");
        }

        [Test]
        public void TextKeyChannel_MultipleKey()
        {
            TextChannel channel = new KeyTextChannel()
                .AddKey(0, "str")
                .AddKey(2, "str2")
                .AddKey(4, "str3");

            AssertCharArrayEquality(channel, 0, "str");
            AssertCharArrayEquality(channel, 2, "str2");
            AssertCharArrayEquality(channel, 4, "str3");
        }

        [Test]
        public void TextKeyChannel_MultipleKeyOverlapping()
        {
            TextChannel channel = new KeyTextChannel()
                .TransitionFromFirstDifference()
                .AddKey(0, "str")
                .AddKey(0, "str2")
                .AddKey(0, "str3")
                .AddKey(2, "str3");

            AssertCharArrayEquality(channel, 1, "str3");
        }

        [Test]
        public void TextKeyChannel_Easing_FromBeginning()
        {
            TextChannel channel = new KeyTextChannel()
                .TransitionFromStart()
                .AddKey(0, "str")
                .AddKey(7, "str2");

            AssertCharArrayEquality(channel, 0, "str");
            AssertCharArrayEquality(channel, 1, "st");
            AssertCharArrayEquality(channel, 2, "s");
            AssertCharArrayEquality(channel, 3, "");
            AssertCharArrayEquality(channel, 4, "s");
            AssertCharArrayEquality(channel, 5, "st");
            AssertCharArrayEquality(channel, 6, "str");
            AssertCharArrayEquality(channel, 7, "str2");
        }

        [Test]
        public void TextKeyChannel_Easing_FromFirstDifference()
        {
            TextChannel channel = new KeyTextChannel()
                .TransitionFromFirstDifference()
                .AddKey(0, "common_abc")
                .AddKey(7, "common_defg");

            AssertCharArrayEquality(channel, 0, "common_abc");
            AssertCharArrayEquality(channel, 1, "common_ab");
            AssertCharArrayEquality(channel, 2, "common_a");
            AssertCharArrayEquality(channel, 3, "common_");
            AssertCharArrayEquality(channel, 4, "common_d");
            AssertCharArrayEquality(channel, 5, "common_de");
            AssertCharArrayEquality(channel, 6, "common_def");
            AssertCharArrayEquality(channel, 7, "common_defg");
        }

        [Test]
        public void ConcatTextChannel()
        {
            TextChannel channel1 = TextChannelBuilder.Constant("str1");
            TextChannel channel2 = TextChannelBuilder.Constant("str2");

            TextChannel concat = new ConcatTextChannel(channel1, channel2);

            AssertCharArrayEquality(concat, 0, "str1str2");
        }

        [TestCase(0, 10, 2, "0")]
        [TestCase(0, 10, 0, "0")]
        [TestCase(0.51f, 10, 0, "1")]
        [TestCase(0.5f, 10, 2, "0.5")]
        [TestCase(0.12345f, 10, 2, "0.12")]
        [TestCase(123.456f, 20, 3, "123.456")]
        [TestCase(-123.456f, 20, 3, "-123.456")]
        [TestCase(123.456f, 20, 0, "123")]
        [TestCase(10001024, 20, 0, "10001024")]
        [TestCase(0.00001f, 10, 2, "0")]
        [TestCase(-0.00001f, 10, 2, "0")]
        public void ValueToTextChannel(float val, int maxLength, int precision, string expected)
        {
            ValueChannel valChannel = new ConstantChannel(val);
            TextChannel channel = new ValueToTextChannel(valChannel, maxLength, precision);
            AssertCharArrayEquality(channel, 0, expected);
        }

        private void AssertCharArrayEquality(TextChannel channel, int timing, string compare)
        {
            StringBuilder builder = new StringBuilder();
            char[] array = channel.ValueAt(timing, out int length);

            for (int i = 0; i < length; i++)
            {
                builder.Append(array[i]);
            }

            Assert.That(builder.ToString(), Is.EqualTo(compare));
        }
    }
}