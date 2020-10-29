using System;
using System.Buffers.Binary;

namespace RTSPPlayerServer.Base.Utilities
{
    /// <summary>
    /// An utility class that provides methods for data conversion.
    /// </summary>
    public static class Conversion
    {
        /// <summary>
        /// Byte order in which data is stored.
        /// </summary>
        public static Endianness Endianness =>
            BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian;

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="Boolean"/> value,
        /// which effectively does nothing for a <see cref="Boolean"/>.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The passed-in value, unmodified.</returns>
        public static bool ReverseEndianness(bool value) => value;

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="Byte"/> value,
        /// which effectively does nothing for a <see cref="Byte"/>.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The passed-in value, unmodified.</returns>
        public static byte ReverseEndianness(byte value) => BinaryPrimitives.ReverseEndianness(value);

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="SByte"/> value,
        /// which effectively does nothing for a <see cref="SByte"/>.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The passed-in value, unmodified.</returns>
        public static sbyte ReverseEndianness(sbyte value) => BinaryPrimitives.ReverseEndianness(value);

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="UInt16"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        public static ushort ReverseEndianness(ushort value) => BinaryPrimitives.ReverseEndianness(value);

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="Int16"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        public static short ReverseEndianness(short value) => BinaryPrimitives.ReverseEndianness(value);

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="UInt32"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        public static uint ReverseEndianness(uint value) => BinaryPrimitives.ReverseEndianness(value);

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="Int32"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        public static int ReverseEndianness(int value) => BinaryPrimitives.ReverseEndianness(value);

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="UInt64"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        public static ulong ReverseEndianness(ulong value) => BinaryPrimitives.ReverseEndianness(value);

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="Int64"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        public static long ReverseEndianness(long value) => BinaryPrimitives.ReverseEndianness(value);

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="Single"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        public static float ReverseEndianness(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes);
        }

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="Double"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        public static double ReverseEndianness(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes);
        }

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="Decimal"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        public static decimal ReverseEndianness(decimal value)
        {
            var bytes = GetBytes(value);
            Array.Reverse(bytes);
            return ToDecimal(bytes);
        }

        /// <summary>
        /// Returns the specified decimal floating point value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 4.</returns>
        private static byte[] GetBytes(decimal value)
        {
            var bits = decimal.GetBits(value);
            var bytes = new byte[sizeof(decimal)];

            bytes[0] = (byte) bits[0];
            bytes[1] = (byte) (bits[0] >> 8);
            bytes[2] = (byte) (bits[0] >> 16);
            bytes[3] = (byte) (bits[0] >> 24);

            bytes[4] = (byte) bits[1];
            bytes[5] = (byte) (bits[1] >> 8);
            bytes[6] = (byte) (bits[1] >> 16);
            bytes[7] = (byte) (bits[1] >> 24);

            bytes[8] = (byte) bits[2];
            bytes[9] = (byte) (bits[2] >> 8);
            bytes[10] = (byte) (bits[2] >> 16);
            bytes[11] = (byte) (bits[2] >> 24);

            bytes[12] = (byte) bits[3];
            bytes[13] = (byte) (bits[3] >> 8);
            bytes[14] = (byte) (bits[3] >> 16);
            bytes[15] = (byte) (bits[3] >> 24);

            return bytes;
        }

        /// <summary>
        /// Converts a read-only byte span into a decimal floating-point value.
        /// </summary>
        /// <param name="value">A read-only span containing the bytes to convert.</param>
        /// <returns>A decimal floating-point value that represents the converted bytes.</returns>
        /// <exception cref="ArgumentException">
        /// The length of value is less than the length of a <see cref="Decimal"/> value.
        /// </exception>
        private static decimal ToDecimal(ReadOnlySpan<byte> value)
        {
            if (value.Length < sizeof(decimal))
                throw new ArgumentException(nameof(value));

            return new decimal(new[]
            {
                BinaryPrimitives.ReadInt32LittleEndian(value),
                BinaryPrimitives.ReadInt32LittleEndian(value.Slice(4)),
                BinaryPrimitives.ReadInt32LittleEndian(value.Slice(8)),
                BinaryPrimitives.ReadInt32LittleEndian(value.Slice(12))
            });
        }
    }
}
