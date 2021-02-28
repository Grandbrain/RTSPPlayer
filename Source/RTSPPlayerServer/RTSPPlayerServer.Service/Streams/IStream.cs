using System;
using RTSPPlayerServer.Service.Base.Primitives;

namespace RTSPPlayerServer.Service.Streams
{
    /// <summary>
    /// An interface that defines an abstract stream.
    /// </summary>
    public interface IStream
    {
        /// <summary>
        /// Stream name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Current status of the stream.
        /// </summary>
        StreamStatus Status { get; }

        /// <summary>
        /// Raised when the stream status changes.
        /// </summary>
        event EventHandler<StreamStatus, string?>? StatusChanged;

        /// <summary>
        /// Starts the stream.
        /// </summary>
        /// <returns>
        /// <c>true</c> true if the stream was successfully started;
        /// <c>false</c> otherwise.
        /// </returns>
        bool TryStart();

        /// <summary>
        /// Starts the stream.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The stream is not finished yet.
        /// </exception>
        void Start();

        /// <summary>
        /// Stops the stream.
        /// </summary>
        void Stop();

        /// <summary>
        /// Waits until the stream finishes work.
        /// </summary>
        void WaitForFinished();
    }
}
