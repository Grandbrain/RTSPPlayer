using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RTSPPlayerServer.Base.IO;
using RTSPPlayerServer.Base.Utilities;

namespace RTSPPlayerServer.Serializers.Network
{
    /// <summary>
    /// A class that provides a network serializer implementation.
    /// </summary>
    public class NetworkSerializer : INetworkSerializer
    {
        /// <summary>
        /// Protocol version to check data integrity.
        /// </summary>
        private static byte ProtocolVersion => 0x01;

        /// <summary>
        /// Packet header size in bytes.
        /// </summary>
        private static int PacketHeaderSize => 40;

        /// <summary>
        /// Packet maximum size in bytes.
        /// </summary>
        private static int PacketMaxSize => 1500;

        /// <summary>
        /// Packet data maximum size in bytes.
        /// </summary>
        private static int PacketDataMaxSize => PacketMaxSize - PacketHeaderSize;

        /// <summary>
        /// Frame data maximum size in bytes.
        /// </summary>
        private static int FrameDataMaxSize => 95_682_560;

        /// <summary>
        /// Task identifier size in bytes.
        /// </summary>
        private static int TaskIdSize => 6;

        /// <summary>
        /// Flow identifier size in bytes.
        /// </summary>
        private static int FlowIdSize => 6;

        /// <summary>
        /// Validates the network frame.
        /// </summary>
        /// <param name="networkFrame">Network frame.</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="networkFrame"/> is valid;
        /// <c>false</c> otherwise.
        /// </returns>
        public bool ValidateFrame(NetworkFrame networkFrame)
        {
            if (networkFrame.DataSegments.Length <= 0
                || string.IsNullOrEmpty(networkFrame.Task)
                || string.IsNullOrEmpty(networkFrame.Flow)
                || Encoding.UTF8.GetByteCount(networkFrame.Task) > TaskIdSize
                || Encoding.UTF8.GetByteCount(networkFrame.Flow) > FlowIdSize)
                return false;

            var size = 0;
            foreach (var segment in networkFrame.DataSegments)
            {
                if (segment.Count <= 0 || FrameDataMaxSize - size < segment.Count)
                    return false;

                size += segment.Count;
            }

            return size <= FrameDataMaxSize;
        }

        /// <summary>
        /// Returns completed network frames.
        /// </summary>
        /// <returns>List of completed network frames.</returns>
        /// <exception cref="NotImplementedException">
        /// This method is not implemented.
        /// </exception>
        public IEnumerable<NetworkFrame> CompletedFrames()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Serializes the network frame into datagrams.
        /// </summary>
        /// <param name="networkFrame">Network frame.</param>
        /// <returns>List of datagrams.</returns>
        /// <exception cref="ArgumentException">
        /// The <paramref name="networkFrame"/> is invalid.
        /// </exception>
        public IEnumerable<byte[]> Serialize(NetworkFrame networkFrame)
        {
            if (!ValidateFrame(networkFrame))
                throw new ArgumentException("The network frame is invalid", nameof(networkFrame));

            int index = 0, packetNumber = 0, frameSize = networkFrame.DataSegments.Sum(e => e.Count);
            int segment = 0, segmentOffset = 0;

            var packets = new List<byte[]>();
            var taskBytes = Encoding.UTF8.GetBytes(networkFrame.Task);
            var flowBytes = Encoding.UTF8.GetBytes(networkFrame.Flow);

            Array.Resize(ref taskBytes, TaskIdSize);
            Array.Resize(ref flowBytes, FlowIdSize);

            while (index < frameSize)
            {
                var dataSize = Math.Min(PacketDataMaxSize, frameSize - index);
                var packet = new byte[PacketHeaderSize + dataSize];
                using var stream = new MemoryStream(packet);
                using var writer = new EndianBinaryWriter(stream);

                writer.Write(ProtocolVersion);
                writer.Write((ushort) 0);
                writer.Write((ushort) packet.Length);
                writer.Write((ushort) packetNumber++);
                writer.Write((uint) index);
                writer.Write(networkFrame.Id);
                writer.Write((uint) frameSize);
                writer.Write(networkFrame.Number);
                writer.Write(networkFrame.Interpretation);
                writer.Write(taskBytes);
                writer.Write(flowBytes);

                while (segment < networkFrame.DataSegments.Length && dataSize > 0)
                {
                    while (segmentOffset >= networkFrame.DataSegments[segment].Count)
                    {
                        ++segment;
                        segmentOffset = 0;
                    }

                    var copySize = Math.Min(dataSize, networkFrame.DataSegments[segment].Count - segmentOffset);
                    writer.Write(networkFrame.DataSegments[segment].Slice(segmentOffset, copySize));

                    segmentOffset += copySize;
                    index += copySize;
                    dataSize -= copySize;
                }

                writer.Seek(1, SeekOrigin.Begin);
                writer.Write(Checksum.Crc16(packet));
                packets.Add(packet);
            }

            return packets;
        }

        /// <summary>
        /// Deserializes a datagram to collect frames.
        /// </summary>
        /// <param name="datagram">Datagram to deserialize.</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="datagram"/> successfully deserialized;
        /// <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// This method is not implemented.
        /// </exception>
        public bool Deserialize(ArraySegment<byte> datagram)
        {
            throw new NotImplementedException();
        }
    }
}
