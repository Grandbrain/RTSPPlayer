using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RTSPPlayerServer.Utilities;

namespace RTSPPlayerServer.Serializers.Network
{
	/// <summary>
	/// A class that provides a network serializer implementation.
	/// </summary>
	internal class NetworkSerializer : INetworkSerializer
	{
		/// <summary>
		/// Specific protocol version to check data integrity.
		/// </summary>
		private static ushort DatagramProtocolVersion => 0x0100;

		/// <summary>
		/// Master chunk identifier code.
		/// </summary>
		private static byte ChunkMasterId => 1;

		/// <summary>
		/// Slave chunk identifier code.
		/// </summary>
		private static byte ChunkSlaveId => 0;

		/// <summary>
		/// RTL answer chunk identifier code.
		/// </summary>
		private static byte ChunkRtlAnswerId => 127;

		/// <summary>
		/// RTL request chunk identifier code.
		/// </summary>
		private static byte ChunkRtlRequestId => 128;

		/// <summary>
		/// Notification chunk identifier code.
		/// </summary>
		private static byte ChunkNotificationId => 129;

		/// <summary>
		/// Datagram header size in bytes.
		/// </summary>
		private static int DatagramHeaderSize => 10;

		/// <summary>
		/// Master chunk header size in bytes.
		/// </summary>
		private static int ChunkMasterHeaderSize => 29;

#if NETWORK_PROTOCOL_EXTENDED
		/// <summary>
		/// Slave chunk header size in bytes.
		/// </summary>
		private static int ChunkSlaveHeaderSize => 29;
#else
		/// <summary>
		/// Slave chunk header size in bytes.
		/// </summary>
		private static int ChunkSlaveHeaderSize => 25;
#endif

		/// <summary>
		/// RTL chunk header size in bytes.
		/// </summary>
		private static int ChunkRtlHeaderSize => 4;

		/// <summary>
		/// Notification chunk header size in bytes.
		/// </summary>
		private static int ChunkNotificationHeaderSize => 3;

		/// <summary>
		/// Chunk task identifier size in bytes.
		/// </summary>
		private static int ChunkTaskSize => 6;

		/// <summary>
		/// Chunk flow identifier size in bytes.
		/// </summary>
		private static int ChunkFlowSize => 6;

		/// <summary>
		/// Frame maximum size without metadata in bytes.
		/// </summary>
		private static int FrameMaxSize => 31850493;

		/// <summary>
		/// Datagram maximum size with metadata in bytes.
		/// </summary>
		private static int DatagramMaxSize => 1500;

		/// <summary>
		/// Chunk maximum size with metadata in bytes.
		/// </summary>
		private static int ChunkMaxSize => 512;

		/// <summary>
		/// Datagram maximum size without metadata in bytes.
		/// </summary>
		private static int DatagramDataMaxSize => DatagramMaxSize - DatagramHeaderSize;

		/// <summary>
		/// Master chunk maximum size without metadata in bytes.
		/// </summary>
		private static int ChunkMasterDataMaxSize => ChunkMaxSize - ChunkMasterHeaderSize;

		/// <summary>
		/// Slave chunk maximum size without metadata in bytes.
		/// </summary>
		private static int ChunkSlaveDataMaxSize => ChunkMaxSize - ChunkSlaveHeaderSize;

		/// <summary>
		/// Validates the network frame.
		/// </summary>
		/// <param name="networkFrame">Network frame.</param>
		/// <returns><c>true</c> if <c>networkFrame</c> is valid; <c>false</c> otherwise.</returns>
		public bool ValidateFrame(NetworkFrame networkFrame)
		{
			return networkFrame?.DataSegments?.Length > 0 &&
			       !string.IsNullOrEmpty(networkFrame.Task) &&
			       !string.IsNullOrEmpty(networkFrame.Flow) &&
			       Encoding.Default.GetByteCount(networkFrame.Task) <= ChunkTaskSize &&
			       Encoding.Default.GetByteCount(networkFrame.Flow) <= ChunkFlowSize &&
			       networkFrame.DataSegments.All(e => e.Count > 0) &&
			       networkFrame.DataSegments.Sum(e => e.Count) <= FrameMaxSize;
		}

		/// <summary>
		/// Returns completed frames.
		/// </summary>
		/// <returns>List of completed frames.</returns>
		/// <exception cref="NotImplementedException"></exception>
		public IEnumerable<NetworkFrame> CompletedFrames()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Serializes the network frame into datagrams.
		/// </summary>
		/// <param name="networkFrame">Network frame.</param>
		/// <returns>List of datagrams.</returns>
		public IEnumerable<byte[]> Serialize(NetworkFrame networkFrame)
		{
			if (!ValidateFrame(networkFrame))
				throw new ArgumentException(nameof(networkFrame));

			int index = 0, slaveNumber = 0, frameSize = networkFrame.DataSegments.Sum(e => e.Count);
			int segment = 0, segmentOffset = 0;

			var dataArrays = new List<byte[]>();
			var taskBytes = Encoding.Default.GetBytes(networkFrame.Task);
			var flowBytes = Encoding.Default.GetBytes(networkFrame.Flow);

			Array.Resize(ref taskBytes, ChunkTaskSize);
			Array.Resize(ref flowBytes, ChunkFlowSize);

			while (index < frameSize)
			{
				int left = frameSize - index, grow = 0, size = DatagramHeaderSize;

				if (index == 0)
				{
					grow = Math.Min(left, ChunkMasterDataMaxSize);
					size += ChunkMasterHeaderSize + grow;
				}

				while (grow < left && DatagramMaxSize - size > ChunkSlaveHeaderSize)
				{
					var freeSize = DatagramMaxSize - ChunkSlaveHeaderSize - size;
					var dataSize = Math.Min(freeSize, ChunkSlaveDataMaxSize);
					var packSize = Math.Min(dataSize, left - grow);
					var allSize = ChunkSlaveHeaderSize + packSize;

					size += allSize;
					grow += packSize;
				}

				dataArrays.Add(new byte[size]);
				using var stream = new MemoryStream(dataArrays.Last());
				using var writer = new BinaryWriter(stream);

				writer.Write(DatagramProtocolVersion);
				writer.Write((ushort) size);
				writer.Write((uint) 0);
				writer.Write((ushort) 0);

				while (stream.Position < stream.Length)
				{
					if (index == 0)
					{
						var freeSize = (int) (stream.Length - stream.Position) - ChunkMasterHeaderSize;
						var dataSize = Math.Min(freeSize, ChunkMasterDataMaxSize);
						var allSize = ChunkMasterHeaderSize + dataSize;

						writer.Write(ChunkMasterId);
						writer.Write((ushort) allSize);
						writer.Write(taskBytes);
						writer.Write(flowBytes);
						writer.Write(networkFrame.Id);
						writer.Write(networkFrame.Interpretation);
						writer.Write(networkFrame.Priority);
						writer.Write(networkFrame.Time);
						writer.Write(networkFrame.Number);
						writer.Write((uint) frameSize);

						while (segment < networkFrame.DataSegments.Length && dataSize > 0)
						{
							var copySize = Math.Min(dataSize, networkFrame.DataSegments[segment].Count - segmentOffset);
							writer.Write(networkFrame.DataSegments[segment].Slice(segmentOffset, copySize));

							segmentOffset += copySize;
							index += copySize;
							dataSize -= copySize;

							if (segmentOffset < networkFrame.DataSegments[segment].Count) continue;
							++segment;
							segmentOffset = 0;
						}
					}
					else
					{
						var freeSize = (int) (stream.Length - stream.Position) - ChunkSlaveHeaderSize;
						var dataSize = Math.Min(freeSize, ChunkSlaveDataMaxSize);
						var allSize = ChunkSlaveHeaderSize + dataSize;

						writer.Write(ChunkSlaveId);
						writer.Write((ushort) allSize);
						writer.Write(taskBytes);
						writer.Write(flowBytes);
						writer.Write(networkFrame.Id);
						writer.Write(networkFrame.Interpretation);
						writer.Write(networkFrame.Priority);
						writer.Write(networkFrame.Time);
						writer.Write((ushort) ++slaveNumber);
#if NETWORK_PROTOCOL_EXTENDED
						writer.Write((uint) index);
#endif
						while (segment < networkFrame.DataSegments.Length && dataSize > 0)
						{
							var copySize = Math.Min(dataSize, networkFrame.DataSegments[segment].Count - segmentOffset);
							writer.Write(networkFrame.DataSegments[segment].Slice(segmentOffset, copySize));

							segmentOffset += copySize;
							index += copySize;
							dataSize -= copySize;

							if (segmentOffset < networkFrame.DataSegments[segment].Count) continue;
							++segment;
							segmentOffset = 0;
						}
					}
				}

				writer.Seek(8, SeekOrigin.Begin);
				writer.Write(ChecksumUtilities.Crc16(dataArrays.Last()));
			}

			return dataArrays;
		}
		
		/// <summary>
		/// Deserializes a datagram to collect frames.
		/// </summary>
		/// <param name="datagram">Datagram to deserialize.</param>
		/// <returns><c>true</c> if <c>datagram</c> successfully deserialized; <c>false</c> otherwise.</returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool Deserialize(ArraySegment<byte> datagram)
		{
			throw new NotImplementedException();
		}
	}
}
