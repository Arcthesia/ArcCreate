using ArcCreate.Gameplay.Scenecontrol;
using NUnit.Framework;

namespace Tests.Unit
{
    public class KeyChannelTest
    {
        [Test]
        public void KeyChannel_OneKey()
        {
            KeyChannel channel = new KeyChannel().AddKey(0, 0, "s");
            Assert.That(channel.ValueAt(0), Is.EqualTo(0));
            Assert.That(channel.ValueAt(1), Is.EqualTo(0));
            Assert.That(channel.ValueAt(-1), Is.EqualTo(0));
        }

        [Test]
        public void KeyChannel_MultipleKeys_AtKey()
        {
            KeyChannel channel = new KeyChannel()
                .AddKey(0, 0)
                .AddKey(1, 1)
                .AddKey(2, 2)
                .AddKey(3, 3);

            Assert.That(channel.ValueAt(0), Is.EqualTo(0));
            Assert.That(channel.ValueAt(1), Is.EqualTo(1));
            Assert.That(channel.ValueAt(2), Is.EqualTo(2));
            Assert.That(channel.ValueAt(3), Is.EqualTo(3));
        }

        [Test]
        public void KeyChannel_MultipleKeys_InBetween()
        {
            KeyChannel channel = new KeyChannel()
                .AddKey(0, 0)
                .AddKey(2, 1);

            Assert.That(channel.ValueAt(1), Is.GreaterThan(0));
            Assert.That(channel.ValueAt(1), Is.LessThan(1));
        }

        [Test]
        public void KeyChannel_MultipleKeysOverlap()
        {
            KeyChannel channel = new KeyChannel()
                .AddKey(0, 0)
                .AddKey(0, 1)
                .AddKey(0, 2)
                .AddKey(2, 2);

            Assert.That(channel.ValueAt(1), Is.EqualTo(2));
        }

        [Test]
        public void KeyChannel_Extrapolate()
        {
            KeyChannel channel = new KeyChannel()
                .SetIntroExtrapolation(true)
                .SetOuttroExtrapolation(true)
                .AddKey(0, 0)
                .AddKey(1, 1);

            Assert.That(channel.ValueAt(-1), Is.LessThan(0));
            Assert.That(channel.ValueAt(2), Is.GreaterThan(1));
        }

        [Test]
        public void KeyChannel_RemoveKey()
        {
            KeyChannel channel = new KeyChannel()
                .AddKey(2, 2)
                .AddKey(3, 3)
                .AddKey(4, 2);

            channel.RemoveKeyAtTiming(3);

            Assert.That(channel.ValueAt(3), Is.EqualTo(2));
        }

        [Test]
        public void KeyChannel_NoKey()
        {
            KeyChannel channel = new KeyChannel();

            Assert.That(channel.ValueAt(0), Is.EqualTo(0));
        }
    }
}