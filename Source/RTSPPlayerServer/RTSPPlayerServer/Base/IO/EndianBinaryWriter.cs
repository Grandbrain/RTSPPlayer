using System;
using System.IO;
using System.Text;
using RTSPPlayerServer.Base.Utilities;

namespace RTSPPlayerServer.Base.IO
{
    /// <summary>
    /// Writes primitive data types as binary values to a stream with a specific encoding and endianness.
    /// </summary>
    public class EndianBinaryWriter : BinaryWriter
    {
        /// <summary>
        /// Stream data endianness.
        /// </summary>
        private readonly Endianness _endianness;

        /// <summary>
        /// Indicates whether the writer should swap bytes according to the endianness.
        /// </summary>
        private bool ReverseEndianness => Conversion.Endianness != _endianness;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndianBinaryWriter"/> class based on the specified stream,
        /// UTF-8 encoding and big-endian endianness.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <exception cref="ArgumentException">
        /// The <paramref name="stream"/> does not support writing or is already closed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="stream"/> is <c>null</c>.
        /// </exception>
        public EndianBinaryWriter(Stream stream) : this(stream, Encoding.UTF8)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EndianBinaryWriter"/> class based on the specified stream,
        /// character encoding and big-endian endianness, and optionally leaves the stream open.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen">
        /// <c>true</c> to leave the stream open after the <see cref="EndianBinaryWriter"/> object is disposed;
        /// <c>false</c> otherwise.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The <paramref name="stream"/> does not support writing or is already closed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="stream"/> or the <paramref name="encoding"/> is <c>null</c>.
        /// </exception>
        private EndianBinaryWriter(Stream stream, Encoding encoding, bool leaveOpen = false)
            : this(stream, encoding, leaveOpen, Endianness.BigEndian)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EndianBinaryWriter"/> class based on the specified stream,
        /// character encoding and endianness, and optionally leaves the stream open.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen">
        /// <c>true</c> to leave the stream open after the <see cref="EndianBinaryWriter"/> object is disposed;
        /// <c>false</c> otherwise.
        /// </param>
        /// <param name="endianness">The stream data endianness.</param>
        /// <exception cref="ArgumentException">
        /// The <paramref name="stream"/> does not support writing or is already closed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="stream"/> or the <paramref name="encoding"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="endianness"/> contains an invalid value.
        /// </exception>
        private EndianBinaryWriter(Stream stream, Encoding encoding, bool leaveOpen, Endianness endianness)
            : base(stream, encoding, leaveOpen)
        {
            _endianness = endianness switch
            {
                Endianness.BigEndian => endianness,
                Endianness.LittleEndian => endianness,
                _ => throw new ArgumentOutOfRangeException(nameof(endianness))
            };
        }

        /// <summary>
        /// Writes a <see cref="Boolean"/> value to the current stream and advances the stream position by one byte.
        /// The bytes of the value are swapped according to the stream data endianness, which effectively does nothing
        /// for a <see cref="Boolean"/>.
        /// </summary>
        /// <param name="value">The <see cref="Boolean"/> value to write.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override void Write(bool value) =>
            base.Write(ReverseEndianness ? Conversion.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes a <see cref="Byte"/> value to the current stream and advances the stream position by one byte.
        /// The bytes of the value are swapped according to the stream data endianness, which effectively does nothing
        /// for a <see cref="Byte"/>.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/> value to write.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override void Write(byte value) =>
            base.Write(ReverseEndianness ? Conversion.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes a <see cref="SByte"/> value to the current stream and advances the stream position by one byte.
        /// The bytes of the value are swapped according to the stream data endianness, which effectively does nothing
        /// for a <see cref="SByte"/>.
        /// </summary>
        /// <param name="value">The <see cref="SByte"/> value to write.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override void Write(sbyte value) =>
            base.Write(ReverseEndianness ? Conversion.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes a <see cref="UInt16"/> value to the current stream and advances the stream position by two bytes.
        /// The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <param name="value">The <see cref="UInt16"/> value to write.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override void Write(ushort value) =>
            base.Write(ReverseEndianness ? Conversion.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes a <see cref="Int16"/> value to the current stream and advances the stream position by two bytes.
        /// The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <param name="value">The <see cref="Int16"/> value to write.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override void Write(short value) =>
            base.Write(ReverseEndianness ? Conversion.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes a <see cref="UInt32"/> value to the current stream and advances the stream position by four bytes.
        /// The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <param name="value">The <see cref="UInt32"/> value to write.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override void Write(uint value) =>
            base.Write(ReverseEndianness ? Conversion.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes a <see cref="Int32"/> value to the current stream and advances the stream position by four bytes.
        /// The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> value to write.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override void Write(int value) =>
            base.Write(ReverseEndianness ? Conversion.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes a <see cref="UInt64"/> value to the current stream and advances the stream position by eight bytes.
        /// The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <param name="value">The <see cref="UInt64"/> value to write.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override void Write(ulong value) =>
            base.Write(ReverseEndianness ? Conversion.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes a <see cref="Int64"/> value to the current stream and advances the stream position by eight bytes.
        /// The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <param name="value">The <see cref="Int64"/> value to write.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override void Write(long value) =>
            base.Write(ReverseEndianness ? Conversion.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes a <see cref="Single"/> value to the current stream and advances the stream position by four bytes.
        /// The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <param name="value">The <see cref="Single"/> value to write.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override void Write(float value) =>
            base.Write(ReverseEndianness ? Conversion.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes a <see cref="Double"/> value to the current stream and advances the stream position by eight bytes.
        /// The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> value to write.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override void Write(double value) =>
            base.Write(ReverseEndianness ? Conversion.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes a <see cref="Decimal"/> value to the current stream and advances the stream position by sixteen
        /// bytes. The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <param name="value">The <see cref="Decimal"/> value to write.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override void Write(decimal value) =>
            base.Write(ReverseEndianness ? Conversion.ReverseEndianness(value) : value);
    }
}
