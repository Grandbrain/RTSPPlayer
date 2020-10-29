using System;
using System.Collections.Generic;

namespace RTSPPlayerServer.Serializers.Network
{
    /// <summary>
    /// An interface that defines a network serializer.
    /// </summary>
    public interface INetworkSerializer
    {
        /// <summary>
        /// Validates the network frame.
        /// </summary>
        /// <param name="networkFrame">Network frame.</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="networkFrame"/> is valid;
        /// <c>false</c> otherwise.
        /// </returns>
        bool ValidateFrame(NetworkFrame networkFrame);

        /// <summary>
        /// Returns completed network frames.
        /// </summary>
        /// <returns>List of completed network frames.</returns>
        /// <exception cref="NotImplementedException">
        /// This method is not implemented.
        /// </exception>
        IEnumerable<NetworkFrame> CompletedFrames();

        /// <summary>
        /// Serializes the network frame into datagrams.
        /// </summary>
        /// <param name="networkFrame">Network frame.</param>
        /// <returns>List of datagrams.</returns>
        /// <exception cref="ArgumentException">
        /// The <paramref name="networkFrame"/> is invalid.
        /// </exception>
        IEnumerable<byte[]> Serialize(NetworkFrame networkFrame);

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
        bool Deserialize(ArraySegment<byte> datagram);
    }
}
