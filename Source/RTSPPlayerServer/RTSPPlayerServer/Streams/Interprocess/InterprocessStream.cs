using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RTSPPlayerServer.Base.Extensions;
using RTSPPlayerServer.Base.Primitives;
using RTSPPlayerServer.Serializers.Interprocess;

namespace RTSPPlayerServer.Streams.Interprocess
{
    /// <summary>
    /// A class that provides interprocess stream implementation.
    /// </summary>
    public class InterprocessStream : StreamTask, IInterprocessStream
    {
        /// <summary>
        /// A thread-safe queue for storing interprocess frames to be sent.
        /// </summary>
        private readonly BlockingCollection<InterprocessFrame>
            _frameQueue = new BlockingCollection<InterprocessFrame>();

        /// <summary>
        /// Interprocess serializer.
        /// </summary>
        private readonly IInterprocessSerializer _interprocessSerializer;

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
        /// Raised when an interprocess frame is received.
        /// </summary>
        public event EventHandler<InterprocessFrame>? FrameReceived;

        /// <summary>
        /// Initializes an interprocess stream with the specified parameters.
        /// </summary>
        /// <param name="name">Stream name.</param>
        /// <param name="interprocessSerializer">Interprocess serializer.</param>
        public InterprocessStream(string name, IInterprocessSerializer interprocessSerializer)
        {
            Name = name;
            _interprocessSerializer = interprocessSerializer;
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

            var task = Task.WhenAny(
                Task.Run(() => SendAsync(cancellationToken), cancellationToken),
                Task.Run(() => ReceiveAsync(cancellationToken), cancellationToken));

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
        /// Sends an interprocess frame to the standard output.
        /// </summary>
        /// <param name="interprocessFrame">Interprocess frame.</param>
        /// <returns>
        /// <c>true</c> if the stream is active and the parameters are valid;
        /// <c>false</c> otherwise.
        /// </returns>
        public bool TrySend(InterprocessFrame interprocessFrame)
        {
            return _status == StreamStatus.Active
                   && _frameQueue.TryAdd(interprocessFrame);
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
        /// Asynchronously sends interprocess frames.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        private async Task SendAsync(CancellationToken cancellationToken)
        {
            await using var outputStream = Console.OpenStandardOutput();
            await using var streamWriter = new StreamWriter(outputStream) {AutoFlush = true};

            while (!_frameQueue.IsCompleted)
            {
                var interprocessFrame = _frameQueue.Take(cancellationToken);
                var message = _interprocessSerializer.Serialize(interprocessFrame);

                await streamWriter.WriteLineAsync(message).WithCancellation(cancellationToken);
            }
        }

        /// <summary>
        /// Asynchronously receives interprocess frames.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        private async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            await using var inputStream = Console.OpenStandardInput();
            using var reader = new StreamReader(inputStream);

            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await reader.ReadLineAsync().WithCancellation(cancellationToken);
                if (string.IsNullOrWhiteSpace(message)) continue;

                var interprocessFrame = _interprocessSerializer.Deserialize(message);
                FrameReceived?.Invoke(this, interprocessFrame);
            }
        }
    }
}
