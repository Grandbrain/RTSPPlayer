using RtspClientSharp.RawFrames;
using RTSPPlayerServer.Base.Primitives;

namespace RTSPPlayerServer.Streams.Media
{
    /// <summary>
    /// An interface that defines a media stream.
    /// </summary>
    public interface IMediaStream : IStream
    {
        /// <summary>
        /// Total number of frames received.
        /// </summary>
        int TotalFramesReceived { get; }

        /// <summary>
        /// Raised when a media frame is received.
        /// </summary>
        event EventHandler<RawFrame, bool>? FrameReceived;
    }
}
