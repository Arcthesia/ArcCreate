using System;
using System.Text;
using ArcCreate.Remote.Common;
using NUnit.Framework;

namespace Tests.Unit
{
    public class MessagePackagerTest
    {
        private MessagePackager messagePackager;

        public byte[] GenerateByteSequence(string str)
        {
            byte[] encoded = Encoding.ASCII.GetBytes(str);
            byte[] byteArray = new byte[encoded.Length + MessagePackager.ControlBytes + MessagePackager.LengthBytes];
            for (int i = 0; i < MessagePackager.ControlBytes; i++)
            {
                byteArray[i] = 0x0;
            }

            byte[] lengthBytes = BitConverter.GetBytes((uint)encoded.Length);
            Array.Copy(lengthBytes, 0, byteArray, MessagePackager.ControlBytes, MessagePackager.LengthBytes);
            Array.Copy(encoded, 0, byteArray, MessagePackager.ControlBytes + MessagePackager.LengthBytes, encoded.Length);
            return byteArray;
        }

        [SetUp]
        public void Setup()
        {
            messagePackager = new MessagePackager();
        }

        [TestCase("Test message")]
        [TestCase("")]
        public void ReadMessageSuccessfully(string msg)
        {
            byte[] bytes = GenerateByteSequence(msg);

            messagePackager.ProcessMessage(bytes, 0, bytes.Length);

            Assert.That(messagePackager.HasQueuedMessage(out RemoteControl rem, out byte[] message));
            string result = Encoding.ASCII.GetString(message);
            Assert.That(result, Is.EqualTo(msg), result);
        }

        [TestCase("Test message")]
        public void ReadMessageWithPausing(string msg)
        {
            byte[] bytes = GenerateByteSequence(msg);

            int halfPoint = bytes.Length / 2;
            messagePackager.ProcessMessage(bytes, 0, halfPoint);
            messagePackager.ProcessMessage(bytes, halfPoint, bytes.Length - halfPoint);

            Assert.That(messagePackager.HasQueuedMessage(out RemoteControl rem, out byte[] message));
            string result = Encoding.ASCII.GetString(message);
            Assert.That(result, Is.EqualTo(msg), result);
        }

        [TestCase("Test message", "Test message 2")]
        [TestCase("", "Test message 2")]
        [TestCase("Test message", "")]
        public void ReadMultipleMessages(string msg1, string msg2)
        {
            byte[] bytes1 = GenerateByteSequence(msg1);
            byte[] bytes2 = GenerateByteSequence(msg2);

            byte[] total = new byte[bytes1.Length + bytes2.Length];
            Array.Copy(bytes1, total, bytes1.Length);
            Array.Copy(bytes2, 0, total, bytes1.Length, bytes2.Length);

            messagePackager.ProcessMessage(total, 0, total.Length);

            Assert.That(messagePackager.HasQueuedMessage(out RemoteControl rem1, out byte[] message1));
            string result1 = Encoding.ASCII.GetString(message1);
            Assert.That(messagePackager.HasQueuedMessage(out RemoteControl rem2, out byte[] message2));
            string result2 = Encoding.ASCII.GetString(message2);

            Assert.That(result1, Is.EqualTo(msg1), result1);
            Assert.That(result2, Is.EqualTo(msg2), result2);
        }

        [Test]
        public void ReadMultipleMessagesWithPausing()
        {
            string msg1 = "Test string";
            byte[] bytes1 = GenerateByteSequence(msg1);

            string msg2 = "Test string2";
            byte[] bytes2 = GenerateByteSequence(msg2);

            byte[] total = new byte[bytes1.Length + bytes2.Length];
            Array.Copy(bytes1, total, bytes1.Length);
            Array.Copy(bytes2, 0, total, bytes1.Length, bytes2.Length);

            messagePackager.ProcessMessage(total, 0, bytes1.Length - 5);
            messagePackager.ProcessMessage(total, bytes1.Length - 5, bytes2.Length + 5);

            Assert.That(messagePackager.HasQueuedMessage(out RemoteControl rem1, out byte[] message1));
            string result1 = Encoding.ASCII.GetString(message1);
            Assert.That(messagePackager.HasQueuedMessage(out RemoteControl rem2, out byte[] message2));
            string result2 = Encoding.ASCII.GetString(message2);

            Assert.That(result1, Is.EqualTo(msg1), result1);
            Assert.That(result2, Is.EqualTo(msg2), result2);
        }

        [TestCase(0)]
        [TestCase(10)]
        [TestCase(99999)]
        public void CreatedHeaderIsDecodable(int length)
        {
            byte[] header = messagePackager.CreateHeader(RemoteControl.Chart, length);
            byte[] bytes = new byte[header.Length + length];
            Array.Copy(header, bytes, header.Length);

            messagePackager.ProcessMessage(bytes, 0, bytes.Length);

            Assert.That(messagePackager.HasQueuedMessage(out RemoteControl rem, out byte[] message));
            Assert.That(rem == RemoteControl.Chart, rem.ToString());
        }
    }
}