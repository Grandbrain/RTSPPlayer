using System;
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
        /// Interprocess serializer.
        /// </summary>
        private readonly IInterprocessSerializer _interprocessSerializer;
        
        /// <summary>
        /// Work task.
        /// </summary>
        private Task _task = Task.CompletedTask;
        
        /// <summary>
        /// Cancellation token source.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Indicates whether the interprocess stream is active.
        /// </summary>
        public bool IsActive => !_cancellationTokenSource?.IsCancellationRequested ?? false;

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
            
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            
            _task = _task.ContinueWith(_ => ReceiveAsync(cancellationToken), cancellationToken).Unwrap();
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
                _task?.Wait(_cancellationTokenSource?.Token ?? new CancellationToken(true));
            }
            catch (OperationCanceledException)
            {

            }
            catch (ObjectDisposedException)
            {
                
            }
        }

        /// <summary>
        /// Asynchronously receives interprocess messages.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        private async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await using var inputStream = Console.OpenStandardInput();
                await using var outputStream = Console.OpenStandardOutput();

                using var reader = new StreamReader(inputStream);
                await using var writer = new StreamWriter(outputStream) {AutoFlush = true};

                while (true)
                {
                    var message = await reader.ReadLineAsync().WithCancellation(cancellationToken);
                    var interprocessFrame = _interprocessSerializer.Deserialize(message);

                    FrameReceived?.Invoke(this, interprocessFrame);
                    
                    await writer.WriteLineAsync(_interprocessSerializer.Serialize(interprocessFrame))
                        .WithCancellation(cancellationToken);
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
