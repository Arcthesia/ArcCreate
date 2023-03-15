using System;
using System.IO;

namespace ArcCreate.Utility.Base62
{
    /// <summary>
    /// Utility that read and write bits in byte array.
    /// </summary>
    public class BitStream : Stream
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BitStream"/> class.
        /// </summary>
        /// <param name="capacity">Capacity of the stream.</param>
        public BitStream(int capacity)
        {
            Source = new byte[capacity];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BitStream"/> class.
        /// </summary>
        /// <param name="source">Source array for the stream.</param>
        public BitStream(byte[] source)
        {
            Source = source;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Gets bit length of the stream.
        /// </summary>
        public override long Length
        {
            get { return Source.Length * 8; }
        }

        /// <summary>
        /// Gets or sets bit position of the stream.
        /// </summary>
        public override long Position { get; set; }

        private byte[] Source { get; set; }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Read the stream to the buffer.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset bit start position of the stream.</param>
        /// <param name="count">Number of bits to read.</param>
        /// <returns>Number of bits read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // Temporary position cursor
            long tempPos = Position;
            tempPos += offset;

            // Buffer byte position and in-byte position
            int readPosCount = 0, readPosMod = 0;

            // Stream byte position and in-byte position
            long posCount = tempPos >> 3;
            int posMod = (int)(tempPos - ((tempPos >> 3) << 3));

            while (tempPos < Position + offset + count && tempPos < Length)
            {
                // Copy the bit from the stream to buffer
                if ((Source[posCount] & (0x1 << (7 - posMod))) != 0)
                {
                    buffer[readPosCount] = (byte)(buffer[readPosCount] | (0x1 << (7 - readPosMod)));
                }
                else
                {
                    buffer[readPosCount] = (byte)(buffer[readPosCount] & (0xffffffff - (0x1 << (7 - readPosMod))));
                }

                // Increment position cursors
                tempPos++;
                if (posMod == 7)
                {
                    posMod = 0;
                    posCount++;
                }
                else
                {
                    posMod++;
                }

                if (readPosMod == 7)
                {
                    readPosMod = 0;
                    readPosCount++;
                }
                else
                {
                    readPosMod++;
                }
            }

            int bits = (int)(tempPos - Position - offset);
            Position = tempPos;
            return bits;
        }

        /// <summary>
        /// Set up the stream position.
        /// </summary>
        /// <param name="offset">Position.</param>
        /// <param name="origin">Position origin.</param>
        /// <returns>Position after setup.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write from buffer to the stream.
        /// </summary>
        /// <param name="buffer">Buffer to write.</param>
        /// <param name="offset">Offset start bit position of buffer.</param>
        /// <param name="count">Number of bits.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            // Temporary position cursor
            long tempPos = Position;

            // Buffer byte position and in-byte position
            int readPosCount = offset >> 3, readPosMod = offset - ((offset >> 3) << 3);

            // Stream byte position and in-byte position
            long posCount = tempPos >> 3;
            int posMod = (int)(tempPos - ((tempPos >> 3) << 3));

            while (tempPos < Position + count && tempPos < Length)
            {
                // Copy the bit from buffer to the stream
                if ((buffer[readPosCount] & (0x1 << (7 - readPosMod))) != 0)
                {
                    Source[posCount] = (byte)(Source[posCount] | (0x1 << (7 - posMod)));
                }
                else
                {
                    Source[posCount] = (byte)(Source[posCount] & (0xffffffff - (0x1 << (7 - posMod))));
                }

                // Increment position cursors
                tempPos++;
                if (posMod == 7)
                {
                    posMod = 0;
                    posCount++;
                }
                else
                {
                    posMod++;
                }

                if (readPosMod == 7)
                {
                    readPosMod = 0;
                    readPosCount++;
                }
                else
                {
                    readPosMod++;
                }
            }

            Position = tempPos;
        }
    }
}