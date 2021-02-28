using System;
using RTSPPlayerServer.Service.Serializers.Interprocess;

namespace RTSPPlayerServer.Service.Streams.Interprocess
{
    /// <summary>
    /// An interface that defines an interprocess stream.
    /// </summary>
    public interface IInterprocessStream : IStream
    {
        /// <summary>
        /// Raised when an interprocess frame is received.
        /// </summary>
        event EventHandler<InterprocessFrame>? FrameReceived;

        /// <summary>
        /// Sends an interprocess frame to the standard output.
        /// </summary>
        /// <param name="interprocessFrame">Interprocess frame.</param>
        /// <returns>
        /// <c>true</c> if the stream is active and the parameters are valid;
        /// <c>false</c> otherwise.
        /// </returns>
        bool TrySend(InterprocessFrame interprocessFrame);
    }
}
