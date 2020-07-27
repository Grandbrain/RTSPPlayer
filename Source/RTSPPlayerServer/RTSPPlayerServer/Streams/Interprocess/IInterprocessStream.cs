using System;
using RTSPPlayerServer.Serializers.Interprocess;

namespace RTSPPlayerServer.Streams.Interprocess
{
    /// <summary>
    /// An interface that defines interprocess stream.
    /// </summary>
    internal interface IInterprocessStream
    {
        /// <summary>
        /// Indicates whether the interprocess stream is active.
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Indicates whether the interprocess stream is healthy.
        /// </summary>
        bool IsHealthy { get; }
        
        /// <summary>
        /// Event handler that processes received interprocess frames.
        /// </summary>
        EventHandler<InterprocessFrame> FrameReceived { get; set; }
        
        /// <summary>
        /// Starts the interprocess stream.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the interprocess stream.
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Waits until the interprocess stream finishes work.
        /// </summary>
        void Wait();
        
        /// <summary>
        /// Sends an interprocess frame to the standard output.
        /// </summary>
        /// <param name="interprocessFrame">Interprocess frame.</param>
        bool TrySend(InterprocessFrame interprocessFrame);
    }
}
