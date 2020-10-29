using System;
using System.Threading;
using System.Threading.Tasks;
using RtspClientSharp;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Audio;
using RtspClientSharp.RawFrames.Video;
using RTSPPlayerServer.Base.Primitives;

namespace RTSPPlayerServer.Streams.Media
{
    /// <summary>
    /// A class that provides a media stream implementation.
    /// </summary>
    public class MediaStream : StreamTask, IMediaStream
    {
        /// <summary>
        /// Time interval to repeat the media metadata.
        /// </summary>
        private readonly TimeSpan _metadataFrequency = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Time span before the next repetition of an unsuccessful operation.
        /// </summary>
        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(5);
        
        /// <summary>
        /// Number of attempts to continue the unsuccessful operation.
        /// </summary>
        private readonly int _numberOfAttempts = 5;

        /// <summary>
        /// Connection parameters.
        /// </summary>
        private readonly ConnectionParameters _connectionParameters;

        /// <summary>
        /// Total number of frames received.
        /// </summary>
        private volatile int _totalFramesReceived;

        /// <summary>
        /// Current status of the stream.
        /// </summary>
        private volatile StreamStatus _status = StreamStatus.Finished;

        /// <summary>
        /// Audio metadata timestamp.
        /// </summary>
        private DateTime _audioMetadataTime = DateTime.MinValue;

        /// <summary>
        /// Video metadata timestamp.
        /// </summary>
        private DateTime _videoMetadataTime = DateTime.MinValue;

        /// <summary>
        /// Stream name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Total number of frames received.
        /// </summary>
        public int TotalFramesReceived => _totalFramesReceived;

        /// <summary>
        /// Current status of the stream.
        /// </summary>
        public StreamStatus Status => _status;

        /// <summary>
        /// Raised when the stream status changes.
        /// </summary>
        public event EventHandler<StreamStatus, string?>? StatusChanged;

        /// <summary>
        /// Raised when a media frame is received.
        /// </summary>
        public event EventHandler<RawFrame, bool>? FrameReceived;

        /// <summary>
        /// Initializes a media stream with the specified parameters.
        /// </summary>
        /// <param name="name">Stream name.</param>
        /// <param name="connectionParameters">Connection parameters.</param>
        /// <param name="metadataFrequency">Time interval to repeat the media metadata.</param>
        /// <param name="retryDelay">Time span before the next repetition of an unsuccessful operation.</param>
        /// <param name="numberOfAttempts">Number of attempts to continue the unsuccessful operation.</param>
        public MediaStream(string name, ConnectionParameters connectionParameters,
            TimeSpan? metadataFrequency = null, TimeSpan? retryDelay = null, int? numberOfAttempts = null)
        {
            Name = name;
            _connectionParameters = connectionParameters;

            if (metadataFrequency.HasValue) _metadataFrequency = metadataFrequency.Value;
            if (retryDelay.HasValue) _retryDelay = retryDelay.Value;
            if (numberOfAttempts.HasValue) _numberOfAttempts = numberOfAttempts.Value;
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

            _audioMetadataTime = DateTime.MinValue;
            _videoMetadataTime = DateTime.MinValue;

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var task = Task.Run(() => ReceiveAsync(cancellationToken), cancellationToken);

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
        /// Raises an appropriate event when a media frame is received.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="mediaFrame">Media frame.</param>
        private void OnFrameReceived(object sender, RawFrame mediaFrame)
        {
            var metadataRequired = false;
            var now = DateTime.UtcNow;

            switch (mediaFrame)
            {
                case RawAudioFrame _ when now >= _audioMetadataTime + _metadataFrequency:
                    metadataRequired = true;
                    _audioMetadataTime = now;
                    break;
                case RawH264Frame _ when now >= _videoMetadataTime + _metadataFrequency:
                    metadataRequired = mediaFrame is RawH264IFrame;
                    _videoMetadataTime = metadataRequired ? now : _videoMetadataTime;
                    break;
                case RawVideoFrame _ when now >= _videoMetadataTime + _metadataFrequency:
                    metadataRequired = true;
                    _videoMetadataTime = now;
                    break;
                case null: return;
            }

            Interlocked.Increment(ref _totalFramesReceived);
            FrameReceived?.Invoke(this, mediaFrame, metadataRequired);
        }

        /// <summary>
        /// Asynchronously connects and receives media frames.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        private async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            using var client = new RtspClient(_connectionParameters);
            client.FrameReceived += OnFrameReceived;

            var remainingConnectAttempts = _numberOfAttempts;
            var remainingReceiveAttempts = _numberOfAttempts;

            while (true)
            {
                try
                {
                    --remainingConnectAttempts;
                    await client.ConnectAsync(cancellationToken);
                }
                catch (Exception)
                {
                    if (remainingConnectAttempts <= 0) throw;

                    await Task.Delay(_retryDelay, cancellationToken);
                    continue;
                }

                remainingConnectAttempts = _numberOfAttempts;

                try
                {
                    --remainingReceiveAttempts;
                    await client.ReceiveAsync(cancellationToken);
                }
                catch (Exception)
                {
                    if (remainingReceiveAttempts <= 0) throw;

                    await Task.Delay(_retryDelay, cancellationToken);
                }
            }
        }
    }
}
