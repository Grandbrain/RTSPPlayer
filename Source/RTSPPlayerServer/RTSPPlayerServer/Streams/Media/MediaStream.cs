using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using RtspClientSharp;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Audio;
using RtspClientSharp.RawFrames.Video;
using RtspClientSharp.Rtsp;
using RTSPPlayerServer.Utilities.Primitives;

namespace RTSPPlayerServer.Streams.Media
{
    /// <summary>
    /// A class that provides media stream implementation.
    /// </summary>
    internal class MediaStream : IMediaStream
    {
        /// <summary>
        /// Time span before the next repetition of an unsuccessful operation.
        /// </summary>
        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Audio metadata timestamp.
        /// </summary>
        private DateTime _audioMetadataTime = DateTime.MinValue;

        /// <summary>
        /// Video metadata timestamp.
        /// </summary>
        private DateTime _videoMetadataTime = DateTime.MinValue;

        /// <summary>
        /// Work task.
        /// </summary>
        private Task _task = Task.CompletedTask;

        /// <summary>
        /// Cancellation token source.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Indicates whether the media stream is active.
        /// </summary>
        public bool IsActive => !_cancellationTokenSource?.IsCancellationRequested ?? false;

        /// <summary>
        /// Contains the total number of frames received.
        /// </summary>
        public long TotalFramesReceived { get; private set; }

        /// <summary>
        /// Media stream connection parameters.
        /// </summary>
        public ConnectionParameters ConnectionParameters { get; }

        /// <summary>
        /// Time interval for repeating media information.
        /// </summary>
        public TimeSpan MetadataFrequency { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Event handler that processes received media info.
        /// </summary>
        public EventHandler<RawFrame, bool> FrameReceived { get; set; }

        /// <summary>
        /// Event handler that processes connection status changes.
        /// </summary>
        public EventHandler<string> ConnectionStatusChanged { get; set; }

        /// <summary>
        /// Constructs a media stream with the specified connection parameters and retry delay.
        /// </summary>
        /// <param name="connectionParameters">Connection parameters.</param>
        /// <param name="retryDelay">Time span before the next repetition of an unsuccessful operation.</param>
        public MediaStream(ConnectionParameters connectionParameters, TimeSpan? retryDelay = null)
        {
            ConnectionParameters = connectionParameters ??
                                   throw new ArgumentNullException(nameof(connectionParameters));

            if (retryDelay.HasValue) _retryDelay = retryDelay.Value;
        }

        /// <summary>
        /// Starts the media stream.
        /// </summary>
        public void Start()
        {
            if (IsActive) return;

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            _audioMetadataTime = DateTime.MinValue;
            _videoMetadataTime = DateTime.MinValue;

            _task = _task.ContinueWith(_ => ReceiveAsync(cancellationToken), cancellationToken).Unwrap();
        }

        /// <summary>
        /// Stops the media stream.
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
        /// Waits until the media stream finishes work.
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
        /// Asynchronously connects and receives media frames.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        private async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var client = new RtspClient(ConnectionParameters);
                client.FrameReceived += OnFrameReceived;

                while (true)
                {
                    OnStatusChanged("Connecting...");

                    try
                    {
                        await client.ConnectAsync(cancellationToken);
                    }
                    catch (InvalidCredentialException)
                    {
                        OnStatusChanged("Invalid login and/or password");
                        await Task.Delay(_retryDelay, cancellationToken);
                        continue;
                    }
                    catch (RtspClientException e)
                    {
                        OnStatusChanged(e.ToString());
                        await Task.Delay(_retryDelay, cancellationToken);
                        continue;
                    }

                    OnStatusChanged("Receiving frames...");

                    try
                    {
                        await client.ReceiveAsync(cancellationToken);
                    }
                    catch (RtspClientException e)
                    {
                        OnStatusChanged(e.ToString());
                        await Task.Delay(_retryDelay, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }
        }

        /// <summary>
        /// Invokes the appropriate event handler when a media frame is received.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="frame">Media frame.</param>
        private void OnFrameReceived(object sender, RawFrame frame)
        {
            var now = DateTime.Now;
            var metadataRequired = false;

            switch (frame)
            {
                case RawAudioFrame _ when now >= _audioMetadataTime + MetadataFrequency:
                    _audioMetadataTime = now;
                    metadataRequired = true;
                    break;
                case RawH264IFrame _ when now >= _videoMetadataTime + MetadataFrequency:
                    _videoMetadataTime = now;
                    metadataRequired = true;
                    break;
            }
            
            ++TotalFramesReceived;
            FrameReceived?.Invoke(this, frame, metadataRequired);
        }

        /// <summary>
        /// Invokes the appropriate event handler when the connection status changes.
        /// </summary>
        /// <param name="status">Connection status.</param>
        private void OnStatusChanged(string status)
        {
            ConnectionStatusChanged?.Invoke(this, status);
        }
    }
}
