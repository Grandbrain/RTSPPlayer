using System;
using System.IO;
using System.Text;
using RTSPPlayerServer.Base.Utilities;

namespace RTSPPlayerServer.Base.IO
{
    /// <summary>
    /// Reads primitive data types as binary values from a stream with a specific encoding and endianness.
    /// </summary>
    public class EndianBinaryReader : BinaryReader
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
        /// Initializes a new instance of the <see cref="EndianBinaryReader"/> class based on the specified stream,
        /// UTF-8 encoding and big-endian endianness.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <exception cref="ArgumentException">
        /// The <paramref name="stream"/> does not support reading or is already closed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="stream"/> is <c>null</c>.
        /// </exception>
        public EndianBinaryReader(Stream stream) : this(stream, Encoding.UTF8)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EndianBinaryReader"/> class based on the specified stream,
        /// character encoding and big-endian endianness, and optionally leaves the stream open.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen">
        /// <c>true</c> to leave the stream open after the <see cref="EndianBinaryReader"/> object is disposed;
        /// <c>false</c> otherwise.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The <paramref name="stream"/> does not support reading or is already closed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="stream"/> or the <paramref name="encoding"/> is <c>null</c>.
        /// </exception>
        private EndianBinaryReader(Stream stream, Encoding encoding, bool leaveOpen = false)
            : this(stream, encoding, leaveOpen, Endianness.BigEndian)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EndianBinaryReader"/> class based on the specified stream,
        /// character encoding and endianness, and optionally leaves the stream open.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen">
        /// <c>true</c> to leave the stream open after the <see cref="EndianBinaryReader"/> object is disposed;
        /// <c>false</c> otherwise.
        /// </param>
        /// <param name="endianness">The stream data endianness.</param>
        /// <exception cref="ArgumentException">
        /// The <paramref name="stream"/> does not support reading or is already closed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="stream"/> or the <paramref name="encoding"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="endianness"/> contains an invalid value.
        /// </exception>
        private EndianBinaryReader(Stream stream, Encoding encoding, bool leaveOpen, Endianness endianness)
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
        /// Reads a <see cref="Boolean"/> value from the current stream and advances the current position of the stream
        /// by one byte. The bytes of the value are swapped according to the stream data endianness, which effectively
        /// does nothing for a <see cref="Boolean"/>.
        /// </summary>
        /// <returns><c>true</c> if the byte is nonzero; <c>false</c> otherwise.</returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override bool ReadBoolean() =>
            ReverseEndianness ? Conversion.ReverseEndianness(base.ReadBoolean()) : base.ReadBoolean();

        /// <summary>
        /// Reads a <see cref="Byte"/> from the current stream and advances the current position of the stream by one
        /// byte. The bytes of the value are swapped according to the stream data endianness, which effectively does
        /// nothing for a <see cref="Byte"/>.
        /// </summary>
        /// <returns>The <see cref="Byte"/> value read from the current stream.</returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override byte ReadByte() =>
            ReverseEndianness ? Conversion.ReverseEndianness(base.ReadByte()) : base.ReadByte();

        /// <summary>
        /// Reads an <see cref="SByte"/> from the current stream and advances the current position of the stream
        /// by one byte. The bytes of the value are swapped according to the stream data endianness, which effectively
        /// does nothing for a <see cref="SByte"/>.
        /// </summary>
        /// <returns>The <see cref="SByte"/> value read from the current stream.</returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override sbyte ReadSByte() =>
            ReverseEndianness ? Conversion.ReverseEndianness(base.ReadSByte()) : base.ReadSByte();

        /// <summary>
        /// Reads an <see cref="UInt16"/> from the current stream and advances the current position of the stream by two
        /// bytes. The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <returns>The <see cref="UInt16"/> value read from the current stream.</returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override ushort ReadUInt16() =>
            ReverseEndianness ? Conversion.ReverseEndianness(base.ReadUInt16()) : base.ReadUInt16();

        /// <summary>
        /// Reads an <see cref="Int16"/> from the current stream and advances the current position of the stream by two
        /// bytes. The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <returns>The <see cref="Int16"/> value read from the current stream.</returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override short ReadInt16() =>
            ReverseEndianness ? Conversion.ReverseEndianness(base.ReadInt16()) : base.ReadInt16();

        /// <summary>
        /// Reads an <see cref="UInt32"/> from the current stream and advances the current position of the stream by
        /// four bytes. The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <returns>The <see cref="UInt32"/> value read from the current stream.</returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override uint ReadUInt32() =>
            ReverseEndianness ? Conversion.ReverseEndianness(base.ReadUInt32()) : base.ReadUInt32();

        /// <summary>
        /// Reads an <see cref="Int32"/> from the current stream and advances the current position of the stream by four
        /// bytes. The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <returns>The <see cref="Int32"/> value read from the current stream.</returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override int ReadInt32() =>
            ReverseEndianness ? Conversion.ReverseEndianness(base.ReadInt32()) : base.ReadInt32();

        /// <summary>
        /// Reads an <see cref="UInt64"/> from the current stream and advances the current position of the stream by
        /// eight bytes. The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <returns>The <see cref="UInt64"/> value read from the current stream.</returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override ulong ReadUInt64() =>
            ReverseEndianness ? Conversion.ReverseEndianness(base.ReadUInt64()) : base.ReadUInt64();

        /// <summary>
        /// Reads an <see cref="Int64"/> from the current stream and advances the current position of the stream by
        /// eight bytes. The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <returns>The <see cref="Int64"/> value read from the current stream.</returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override long ReadInt64() =>
            ReverseEndianness ? Conversion.ReverseEndianness(base.ReadInt64()) : base.ReadInt64();

        /// <summary>
        /// Reads a <see cref="Single"/> from the current stream and advances the current position of the stream by four
        /// bytes. The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <returns>The <see cref="Single"/> value read from the current stream.</returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override float ReadSingle() =>
            ReverseEndianness ? Conversion.ReverseEndianness(base.ReadSingle()) : base.ReadSingle();

        /// <summary>
        /// Reads a <see cref="Double"/> from the current stream and advances the current position of the stream by
        /// eight bytes. The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <returns>The <see cref="Double"/> value read from the current stream.</returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override double ReadDouble() =>
            ReverseEndianness ? Conversion.ReverseEndianness(base.ReadDouble()) : base.ReadDouble();

        /// <summary>
        /// Reads a <see cref="Decimal"/> from the current stream and advances the current position of the stream by
        /// sixteen bytes. The bytes of the value are swapped according to the stream data endianness.
        /// </summary>
        /// <returns>The <see cref="Decimal"/> value read from the current stream.</returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public override decimal ReadDecimal() =>
            ReverseEndianness ? Conversion.ReverseEndianness(base.ReadDecimal()) : base.ReadDecimal();
    }
}
