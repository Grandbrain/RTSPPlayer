using System;
using System.Collections.Generic;

namespace RTSPPlayerServer.Serializers.Network
{
    /// <summary>
    /// Interface that defines a network serializer.
    /// </summary>
    internal interface INetworkSerializer
    {
        /// <summary>
        /// Validates the network frame.
        /// </summary>
        /// <param name="networkFrame">Network frame.</param>
        /// <returns><c>true</c> if <c>networkFrame</c> is valid; <c>false</c> otherwise.</returns>
        bool ValidateFrame(NetworkFrame networkFrame);

        /// <summary>
        /// Returns completed frames.
        /// </summary>
        /// <returns>List of completed frames.</returns>
        IEnumerable<NetworkFrame> CompletedFrames();

        /// <summary>
        /// Serializes the network frame into datagrams.
        /// </summary>
        /// <param name="networkFrame">Network frame.</param>
        /// <returns>List of datagrams.</returns>
        IEnumerable<byte[]> Serialize(NetworkFrame networkFrame);

        /// <summary>
        /// Deserializes a datagram to collect frames.
        /// </summary>
        /// <param name="datagram">Datagram to deserialize.</param>
        /// <returns><c>true</c> if <c>datagram</c> successfully deserialized; <c>false</c> otherwise.</returns>
        bool Deserialize(ArraySegment<byte> datagram);
    }
}
