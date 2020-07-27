using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RTSPPlayerServer.Serializers.Interprocess;
using RTSPPlayerServer.Utilities.Extensions;

namespace RTSPPlayerServer.Streams.Interprocess
{
    /// <summary>
    /// A class that provides interprocess stream implementation.
    /// </summary>
    internal class InterprocessStream : IInterprocessStream
    {
        /// <summary>
        /// A thread-safe collection for storing interprocess frames to be sent.
        /// </summary>
        private readonly BlockingCollection<InterprocessFrame> _blockingCollection = 
            new BlockingCollection<InterprocessFrame>();
        
        /// <summary>
        /// Interprocess serializer.
        /// </summary>
        private readonly IInterprocessSerializer _interprocessSerializer;
        
        /// <summary>
        /// Asynchronous task to receive interprocess frames.
        /// </summary>
        private Task _receiveTask = Task.CompletedTask;
        
        /// <summary>
        /// Asynchronous task to send interprocess frames.
        /// </summary>
        private Task _sendTask = Task.CompletedTask;
        
        /// <summary>
        /// Cancellation token source.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Indicates whether the interprocess stream is active.
        /// </summary>
        public bool IsActive => !_cancellationTokenSource?.IsCancellationRequested ?? false;
        
        /// <summary>
        /// Indicates whether the interprocess stream is healthy.
        /// </summary>
        public bool IsHealthy { get; private set; } = true;

        /// <summary>
        /// Event handler that processes received interprocess frames.
        /// </summary>
        public EventHandler<InterprocessFrame> FrameReceived { get; set; }
        
        /// <summary>
        /// Constructs an interprocess stream with the specified interprocess serializer.
        /// </summary>
        /// <param name="interprocessSerializer">Interprocess serializer.</param>
        public InterprocessStream(IInterprocessSerializer interprocessSerializer)
        {
            _interprocessSerializer = interprocessSerializer;
        }

        /// <summary>
        /// Starts the interprocess stream.
        /// </summary>
        public void Start()
        {
            if (IsActive) return;

            IsHealthy = true;
            
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            
            _receiveTask = _receiveTask.ContinueWith(_ => ReceiveAsync(cancellationToken), cancellationToken).Unwrap();
            _sendTask = _sendTask.ContinueWith(_ => SendAsync(cancellationToken), cancellationToken).Unwrap();
        }

        /// <summary>
        /// Stops the interprocess stream.
        /// </summary>
        public void Stop()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            }
            catch (ObjectDisposedException)
            {

            }
        }

        /// <summary>
        /// Waits until the interprocess stream finishes work.
        /// </summary>
        public void Wait()
        {
            try
            {
                _receiveTask?.Wait(_cancellationTokenSource?.Token ?? new CancellationToken(true));
                _sendTask?.Wait(_cancellationTokenSource?.Token ?? new CancellationToken(true));
            }
            catch (OperationCanceledException)
            {

            }
            catch (ObjectDisposedException)
            {
                
            }
        }

        /// <summary>
        /// Sends an interprocess frame to the standard output.
        /// </summary>
        /// <param name="interprocessFrame">Interprocess frame.</param>
        public bool TrySend(InterprocessFrame interprocessFrame)
        {
            return _blockingCollection.TryAdd(interprocessFrame);
        }

        /// <summary>
        /// Asynchronously receives interprocess frames.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        private async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await using var inputStream = Console.OpenStandardInput();
                using var reader = new StreamReader(inputStream);

                while (true)
                {
                    var message = await reader.ReadLineAsync().WithCancellation(cancellationToken);
                    var interprocessFrame = _interprocessSerializer.Deserialize(message);

                    FrameReceived?.Invoke(this, interprocessFrame);
                }
            }
            catch (OperationCanceledException)
            {
                
            }
            catch (Exception)
            {
                IsHealthy = false;
                Stop();
            }
        }
        /// <summary>
        /// Asynchronously sends interprocess frames.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        private async Task SendAsync(CancellationToken cancellationToken)
        {
            try
            {
                await using var outputStream = Console.OpenStandardOutput();
                await using var writer = new StreamWriter(outputStream) {AutoFlush = true};

                while (true)
                {
                    var interprocessFrame = _blockingCollection.Take(cancellationToken);
                    var message = _interprocessSerializer.Serialize(interprocessFrame);

                    await writer.WriteLineAsync(message).WithCancellation(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                
            }
            catch (Exception)
            {
                IsHealthy = false;
                Stop();
            }
        }
    }
}
