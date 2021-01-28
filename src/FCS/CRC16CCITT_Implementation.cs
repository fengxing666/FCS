using System;
using System.IO;

namespace FCS
{
    public static class CRC16CCITT_Implementation
    {
        /// <summary>
        /// The initial CRC remainder as specified by the CRC-CCITT (Kermit) protocol. 
        /// </summary>
        public const ushort InitialCrcRemainder = 0x0;

        /// <summary>
        /// The generator polynomial defined for the CRC-CCITT (Kermit) algorithm.
        /// </summary>
        /// <remarks>
        ///  The polynomial x16 + x12 + x5 + 1 is represented as
        ///  1 0001 0000 0010 0001
        ///  The high order bit (x^16) is always on, so don't need to store it (it is implied by the algorithm).  
        ///  => 0001 0000 0010 0001 
        /// Invert the order because this implementation works on the bits in reverse order (LSB to MSB)
        ///  => 1000 0100 0000 1000 => 0x8408
        /// </remarks>
        private const int KermitGeneratorPolynomial = 0x8408;

        /// <summary>
        /// The size of the precalculated byte CRC table (256 possible values for an 8 bit byte);
        /// </summary>
        private const int ByteTableSize = 256;

        /// <summary>
        /// Precalculated table of byte CRC's
        /// </summary>
        static readonly ushort[] ByteCrcTable = new ushort[ByteTableSize];

        static CRC16CCITT_Implementation()
        {
            for (int i = 0; i < ByteTableSize; ++i)
            {
                ushort crc = InitialCrcRemainder;
                ushort data = (ushort)i;
                for (int j = 0; j < 8; ++j)
                {
                    if (((crc ^ data) & 0x0001) != 0)
                    {
                        crc = (ushort)((crc >> 1) ^ KermitGeneratorPolynomial);
                    }
                    else
                    {
                        crc >>= 1;
                    }
                    data >>= 1;
                }
                ByteCrcTable[i] = crc;
            }
        }
        /// <summary>
        /// Computes the CRC for the given byte array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>the computed CRC value as an unsigned short</returns>
        public static ushort ComputeCrc(byte[] bytes)
        {
            return ComputeCrc(bytes, InitialCrcRemainder, 0, bytes.LongLength);
        }

        public static ushort ComputeCrc(Stream stream)
        {
            return ComputeCrc(stream, InitialCrcRemainder, 0, stream.Length);
        }
        public static ushort ComputeCrc(byte[] bytes, ushort initial)
        {
            return ComputeCrc(bytes, initial, 0, bytes.LongLength);
        }

        public static ushort ComputeCrc(Stream stream, ushort initial)
        {
            return ComputeCrc(stream, initial, 0, stream.Length);
        }

        /// <summary>
        /// Computes the checkSum for the given byte array with offset and count based on initial crc.
        /// </summary>
        /// <param name="initial">The initial value of crc</param>
        /// <param name="bytes">byte array to compute crc of</param>
        /// <param name="offset">the offset at which to start computing</param>
        /// <param name="count">the number of bytes to compute</param>
        /// <returns>the computed CRC value as an unsigned short</returns>
        /// <remarks>Used for iterating over a byte stream so doesn't have to be all loaded into memory at once.</remarks>
        public static ushort ComputeCrc(byte[] bytes, ushort initial, long offset, long count)
        {
            ushort crc = initial;
            long end = offset + count;
            // Divide the message by the polynomial, a byte at a time.
            for (long j = offset; j < end; j++)
            {
                byte data = (byte)((crc & 0xff) ^ bytes[j]);
                crc = (ushort)(ByteCrcTable[data] ^ (crc >> 8));
            }
            return crc;
        }

        public static ushort ComputeCrc(Stream stream, ushort initial, long offset, long count)
        {
            if (count <= int.MaxValue)
            {
                stream.Seek(offset, SeekOrigin.Begin);
                byte[] bytes = new byte[count];
                stream.Read(bytes, 0, Convert.ToInt32(count));
                return ComputeCrc(bytes, initial, 0, count);
            }
            else
            {
                long readCount = 0;
                while (readCount < count)
                {
                    int read = (count - readCount) >= int.MaxValue ? int.MaxValue : Convert.ToInt32(count - readCount);
                    initial = ComputeCrc(stream, initial, offset + readCount, read);
                    readCount += read;
                }
                return initial;
            }
        }
    }
}
