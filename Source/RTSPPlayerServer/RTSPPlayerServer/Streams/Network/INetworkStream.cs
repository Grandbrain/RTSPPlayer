using System.Net;
using RTSPPlayerServer.Serializers.Network;

namespace RTSPPlayerServer.Streams.Network
{
    /// <summary>
    /// An interface that defines a network stream.
    /// </summary>
    public interface INetworkStream : IStream
    {
        /// <summary>
        /// Sends a network frame to the specified endpoint.
        /// </summary>
        /// <param name="networkFrame">Network frame.</param>
        /// <param name="networkEndpoint">Network endpoint.</param>
        /// <returns>
        /// <c>true</c> if the stream is active and the parameters are valid;
        /// <c>false</c> otherwise.
        /// </returns>
        bool TrySend(NetworkFrame networkFrame, IPEndPoint networkEndpoint);
    }
}
