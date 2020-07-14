using System;
using RtspClientSharp;
using RtspClientSharp.RawFrames;
using RTSPPlayerServer.Utilities.Primitives;

namespace RTSPPlayerServer.Streams.Media
{
    /// <summary>
    /// An interface that defines media stream.
    /// </summary>
    internal interface IMediaStream
    {
        /// <summary>
        /// Indicates whether the media stream is active.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Contains the total number of frames received.
        /// </summary>
        long TotalFramesReceived { get; }

        /// <summary>
        /// Media stream connection parameters.
        /// </summary>
        ConnectionParameters ConnectionParameters { get; }

        /// <summary>
        /// Time interval for repeating media metadata.
        /// </summary>
        TimeSpan MetadataFrequency { get; set; }
            
        /// <summary>
        /// Event handler that processes received media metadata.
        /// </summary>
        EventHandler<RawFrame, bool> FrameReceived { get; set; }

        /// <summary>
        /// Event handler that processes connection status changes.
        /// </summary>
        EventHandler<string> ConnectionStatusChanged { get; set; }

        /// <summary>
        /// Starts the media stream.
        /// </summary>
        void Start();
        
        /// <summary>
        /// Stops the media stream.
        /// </summary>
        void Stop();

        /// <summary>
        /// Waits until the media stream finishes work.
        /// </summary>
        void Wait();
    }
}
