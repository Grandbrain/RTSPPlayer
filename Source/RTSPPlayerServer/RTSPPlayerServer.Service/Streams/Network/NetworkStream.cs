using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RTSPPlayerServer.Service.Base.Extensions;
using RTSPPlayerServer.Service.Base.Primitives;
using RTSPPlayerServer.Service.Serializers.Network;

namespace RTSPPlayerServer.Service.Streams.Network
{
    /// <summary>
    /// A class that provides a network stream implementation.
    /// </summary>
    public class NetworkStream : StreamTask, INetworkStream
    {
        /// <summary>
        /// A thread-safe queue for storing network frames to be sent.
        /// </summary>
        private readonly BlockingCollection<(NetworkFrame, IPEndPoint)> _frameQueue =
            new BlockingCollection<(NetworkFrame, IPEndPoint)>();

        /// <summary>
        /// Network serializer.
        /// </summary>
        private readonly INetworkSerializer _networkSerializer;

        /// <summary>
        /// Current status of the stream.
        /// </summary>
        private volatile StreamStatus _status = StreamStatus.Finished;

        /// <summary>
        /// Stream name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Current status of the stream.
        /// </summary>
        public StreamStatus Status => _status;

        /// <summary>
        /// Raised when the stream status changes.
        /// </summary>
        public event EventHandler<StreamStatus, string?>? StatusChanged;

        /// <summary>
        /// Initializes a network stream with the specified parameters.
        /// </summary>
        /// <param name="name">Stream name.</param>
        /// <param name="networkSerializer">Network serializer.</param>
        public NetworkStream(string name, INetworkSerializer networkSerializer)
        {
            Name = name;
            _networkSerializer = networkSerializer;
        }

        /// <summary>
        /// Starts the stream.
        /// </summary>
        /// <returns>
        /// <c>true</c> true if the stream was successfully started;
        /// <c>false</c> otherwise.
        /// </returns>
        public bool TryStart()
        {
            if (!Task.IsCompleted) return false;

            Start();
            return true;
        }

        /// <summary>
        /// Starts the stream.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The stream is not finished yet.
        /// </exception>
        public void Start()
        {
            if (!Task.IsCompleted)
                throw new InvalidOperationException("The stream cannot be started because it has not finished yet.");

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var task = Task.Run(() => SendAsync(cancellationToken), cancellationToken);

            base.Start(task, cancellationTokenSource);
        }

        /// <summary>
        /// Stops the stream.
        /// </summary>
        public new void Stop() => base.Stop();

        /// <summary>
        /// Waits until the stream finishes work.
        /// </summary>
        public new void WaitForFinished() => base.WaitForFinished();

        /// <summary>
        /// Sends a network frame to the specified endpoint.
        /// </summary>
        /// <param name="networkFrame">Network frame.</param>
        /// <param name="networkEndpoint">Network endpoint.</param>
        /// <returns>
        /// <c>true</c> if the stream is active and the parameters are valid;
        /// <c>false</c> otherwise.
        /// </returns>
        public bool TrySend(NetworkFrame networkFrame, IPEndPoint networkEndpoint)
        {
            return _status == StreamStatus.Active
                   && _networkSerializer.ValidateFrame(networkFrame)
                   && _frameQueue.TryAdd((networkFrame, networkEndpoint));
        }

        /// <summary>
        /// Raises an appropriate event when the status of the stream changes.
        /// </summary>
        /// <param name="status">Task status.</param>
        /// <param name="message">Error message.</param>
        protected override void OnStatusChanged(TaskStatus status, string? message)
        {
            var streamStatus = status switch
            {
                TaskStatus.RanToCompletion => StreamStatus.Finished,
                TaskStatus.Canceled => StreamStatus.Canceled,
                TaskStatus.Faulted => StreamStatus.Faulted,
                _ => StreamStatus.Active
            };

            StatusChanged?.Invoke(this, _status = streamStatus, message);
        }

        /// <summary>
        /// Asynchronously sends network frames.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        private async Task SendAsync(CancellationToken cancellationToken)
        {
            using var udpClient = new UdpClient();

            while (!_frameQueue.IsCompleted)
            {
                var (networkFrame, endPoint) = _frameQueue.Take(cancellationToken);
                var packets = _networkSerializer.Serialize(networkFrame);

                foreach (var packet in packets)
                {
                    await udpClient.SendAsync(packet, packet.Length, endPoint)
                        .WithCancellation(cancellationToken);
                }
            }
        }
    }
}
