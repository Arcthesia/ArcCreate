using System.IO;
using System.Text;

namespace ArcCreate.Utility.Base62
{
    public static class EncodingExtensions
    {
        private const string Base62CodingSpace = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Convert a byte array.
        /// </summary>
        /// <param name="original">Byte array.</param>
        /// <returns>Base62 string.</returns>
        public static string ToBase62(this byte[] original)
        {
            StringBuilder sb = new StringBuilder();
            BitStream stream = new BitStream(original);         // Set up the BitStream
            byte[] read = new byte[1];                          // Only read 6-bit at a time
            while (true)
            {
                read[0] = 0;
                int length = stream.Read(read, 0, 6);           // Try to read 6 bits

                // Not reaching the end
                if (length == 6)
                {
                    // First 5-bit is 11111
                    if ((int)(read[0] >> 3) == 0x1f)
                    {
                        sb.Append(Base62CodingSpace[61]);

                        // Leave the 6th bit to next grou
                        stream.Seek(-1, SeekOrigin.Current);
                    }

                    // First 5-bit is 11110
                    else if (read[0] >> 3 == 0x1e)
                    {
                        sb.Append(Base62CodingSpace[60]);
                        stream.Seek(-1, SeekOrigin.Current);
                    }

                    // Encode 6-bit
                    else
                    {
                        sb.Append(Base62CodingSpace[read[0] >> 2]);
                    }
                }

                // Reached the end completely
                else if (length == 0)
                {
                    break;
                }

                // Reached the end with some bits left
                else
                {
                    // Padding 0s to make the last bits to 6 bit
                    sb.Append(Base62CodingSpace[read[0] >> (8 - length)]);
                    break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Convert a Base62 string to byte array.
        /// </summary>
        /// <param name="base62">Base62 string.</param>
        /// <returns>Byte array.</returns>
        public static byte[] FromBase62(this string base62)
        {
            // Character count
            int count = 0;

            // Set up the BitStream
            BitStream stream = new BitStream(base62.Length * 6 / 8);

            foreach (char c in base62)
            {
                // Look up coding table
                int index = Base62CodingSpace.IndexOf(c);

                // If end is reached
                if (count == base62.Length - 1)
                {
                    // Check if the ending is good
                    int mod = (int)(stream.Position % 8);
                    if (mod == 0)
                    {
                        throw new InvalidDataException("an extra character was found");
                    }

                    if ((index >> (8 - mod)) > 0)
                    {
                        throw new InvalidDataException("invalid ending character was found");
                    }

                    stream.Write(new byte[] { (byte)(index << mod) }, 0, 8 - mod);
                }
                else
                {
                    // If 60 or 61 then only write 5 bits to the stream, otherwise 6 bits.
                    if (index == 60)
                    {
                        stream.Write(new byte[] { 0xf0 }, 0, 5);
                    }
                    else if (index == 61)
                    {
                        stream.Write(new byte[] { 0xf8 }, 0, 5);
                    }
                    else
                    {
                        stream.Write(new byte[] { (byte)index }, 2, 6);
                    }
                }

                count++;
            }

            // Dump out the bytes
            byte[] result = new byte[stream.Position / 8];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(result, 0, result.Length * 8);
            return result;
        }
    }
}