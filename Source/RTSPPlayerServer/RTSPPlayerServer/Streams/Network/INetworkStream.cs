using System.Net;
using RTSPPlayerServer.Serializers.Network;

namespace RTSPPlayerServer.Streams.Network
{
    /// <summary>
    /// Interface that defines a network stream.
    /// </summary>
    internal interface INetworkStream
    {
        /// <summary>
        /// Indicates whether the network stream is active.
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Indicates whether the network stream is healthy.
        /// </summary>
        bool IsHealthy { get; }
        
        /// <summary>
        /// Starts the network stream.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the network stream.
        /// </summary>
        void Stop();

        /// <summary>
        /// Waits until the network stream finishes work.
        /// </summary>
        void Wait();

        /// <summary>
        /// Sends a network frame to the specified endpoint.
        /// </summary>
        /// <param name="networkFrame">Network frame.</param>
        /// <param name="endPoint">End point.</param>
        bool TrySend(NetworkFrame networkFrame, EndPoint endPoint);
    }
}
