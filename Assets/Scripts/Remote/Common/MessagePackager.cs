using System;
using System.Collections.Generic;

namespace ArcCreate.Remote.Common
{
    // Message layout is:
    // |-2 bytes-|-4 bytes-|-(Length)bytes-|
    // |-Control-|-Length--|-----Data------|
    // This supports: 256 control signal, 4GB transfer.
    public class MessagePackager
    {
        public const int ControlBytes = 2;
        public const int LengthBytes = 4;

        private readonly Queue<(RemoteControl control, byte[] message)> messageQueue = new Queue<(RemoteControl control, byte[] message)>();

        private readonly byte[] headerBytes = new byte[ControlBytes + LengthBytes];
        private byte[] messageBytes;
        private int headerBytesSoFar = 0;
        private int messageBytesSoFar = 0;

        public void ProcessMessage(byte[] buffer, int offset, int length)
        {
            if (length <= 0)
            {
                return;
            }

            length += offset;

            int bufferIndex = offset;
            for (int i = headerBytesSoFar; i < Math.Min(headerBytes.Length, length); i++)
            {
                headerBytes[i] = buffer[bufferIndex];
                bufferIndex += 1;
            }

            headerBytesSoFar = Math.Min(headerBytes.Length, length);

            if (messageBytes == null && headerBytesSoFar == headerBytes.Length)
            {
                uint messageLength = BitConverter.ToUInt32(headerBytes, ControlBytes);
                if (messageLength == 0)
                {
                    byte[] controlBytes = new byte[ControlBytes];
                    Array.Copy(headerBytes, controlBytes, ControlBytes);
                    short controlInt = BitConverter.ToInt16(controlBytes, 0);
                    RemoteControl control = (RemoteControl)controlInt;

                    messageQueue.Enqueue((control, new byte[0]));
                    messageBytes = null;
                    headerBytesSoFar = 0;
                    messageBytesSoFar = 0;
                    ProcessMessage(buffer, bufferIndex, length - bufferIndex);
                    return;
                }
                else
                {
                    messageBytes = new byte[messageLength];
                    messageBytesSoFar = 0;
                }
            }

            if (messageBytes != null)
            {
                for (int i = bufferIndex; i < length; i++)
                {
                    messageBytes[messageBytesSoFar] = buffer[i];
                    messageBytesSoFar += 1;

                    if (messageBytesSoFar == messageBytes.Length)
                    {
                        byte[] controlBytes = new byte[ControlBytes];
                        Array.Copy(headerBytes, controlBytes, ControlBytes);
                        short controlInt = BitConverter.ToInt16(controlBytes, 0);
                        RemoteControl control = (RemoteControl)controlInt;

                        messageQueue.Enqueue((control, messageBytes));
                        messageBytes = null;
                        headerBytesSoFar = 0;
                        messageBytesSoFar = 0;
                        ProcessMessage(buffer, i + 1, length - i - 1);
                        return;
                    }
                }
            }
        }

        public bool HasQueuedMessage(out RemoteControl control, out byte[] message)
        {
            if (messageQueue.Count > 0)
            {
                (control, message) = messageQueue.Dequeue();
                return true;
            }

            control = RemoteControl.Invalid;
            message = null;
            return false;
        }

        public byte[] CreateHeader(RemoteControl control, int length)
        {
            byte[] result = new byte[ControlBytes + LengthBytes];

            byte[] controlBytes = BitConverter.GetBytes((short)control);
            byte[] lengthBytes = BitConverter.GetBytes((uint)length);

            Array.Copy(controlBytes, 0, result, 0, ControlBytes);
            Array.Copy(lengthBytes, 0, result, ControlBytes, LengthBytes);

            return result;
        }
    }
}