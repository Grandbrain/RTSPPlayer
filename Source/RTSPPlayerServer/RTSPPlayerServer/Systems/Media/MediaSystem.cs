using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RtspClientSharp;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Audio;
using RtspClientSharp.RawFrames.Video;
using RTSPPlayerServer.Base.IO;
using RTSPPlayerServer.Base.Primitives;
using RTSPPlayerServer.Base.Utilities;
using RTSPPlayerServer.Serializers.Interprocess;
using RTSPPlayerServer.Serializers.Network;
using RTSPPlayerServer.Streams;
using RTSPPlayerServer.Streams.Interprocess;
using RTSPPlayerServer.Streams.Media;
using RTSPPlayerServer.Streams.Network;

namespace RTSPPlayerServer.Systems.Media
{
    /// <summary>
    /// A class that provides a media system implementation.
    /// </summary>
    public class MediaSystem : SystemTask, IMediaSystem
    {
        /// <summary>
        /// Media system playlist.
        /// </summary>
        private readonly MediaSystemPlaylist _mediaSystemPlaylist = new MediaSystemPlaylist();

        /// <summary>
        /// Interprocess stream.
        /// </summary>
        private readonly IInterprocessStream _interprocessStream;

        /// <summary>
        /// Network stream.
        /// </summary>
        private readonly INetworkStream _networkStream;

        /// <summary>
        /// Network endpoint.
        /// </summary>
        private volatile IPEndPoint? _endPoint;

        /// <summary>
        /// Current status of the system.
        /// </summary>
        private volatile SystemStatus _status = SystemStatus.Finished;

        /// <summary>
        /// System name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Current status of the system.
        /// </summary>
        public SystemStatus Status => _status;

        /// <summary>
        /// Raised when the system status changes.
        /// </summary>
        public event EventHandler<SystemStatus, string?>? StatusChanged;

        /// <summary>
        /// Initializes a media system with the specified parameters.
        /// </summary>
        /// <param name="name">System name.</param>
        /// <param name="interprocessStream">Interprocess stream.</param>
        /// <param name="networkStream">Network stream.</param>
        public MediaSystem(string name, IInterprocessStream interprocessStream, INetworkStream networkStream)
        {
            Name = name;
            _interprocessStream = interprocessStream;
            _networkStream = networkStream;

            _interprocessStream.StatusChanged += OnStatusChanged;
            _interprocessStream.FrameReceived += OnFrameReceived;
            _networkStream.StatusChanged += OnStatusChanged;
        }

        /// <summary>
        /// Starts the system.
        /// </summary>
        /// <returns>
        /// <c>true</c> true if the system was successfully started;
        /// <c>false</c> otherwise.
        /// </returns>
        public bool TryStart()
        {
            if (!Task.IsCompleted) return false;

            Start();
            return true;
        }

        /// <summary>
        /// Starts the system.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The system is not finished yet.
        /// </exception>
        public new void Start()
        {
            if (!Task.IsCompleted)
                throw new InvalidOperationException("The system cannot be started because it has not finished yet.");

            base.Start();
            _mediaSystemPlaylist.StartAll();
        }

        /// <summary>
        /// Stops the system.
        /// </summary>
        public new void Stop()
        {
            base.Stop();
            _mediaSystemPlaylist.StopAll();
        }

        /// <summary>
        /// Stops the system with an error.
        /// </summary>
        /// <param name="message">Error message.</param>
        public new void StopWithError(string message)
        {
            base.StopWithError(message);
            _mediaSystemPlaylist.StopAll();
        }

        /// <summary>
        /// Waits until the system finishes work.
        /// </summary>
        public new void WaitForFinished()
        {
            base.WaitForFinished();
            _mediaSystemPlaylist.WaitAll();
        }

        /// <summary>
        /// Raises an appropriate event when the status of the system changes.
        /// </summary>
        /// <param name="status">Task status.</param>
        /// <param name="message">Error message.</param>
        protected override void OnStatusChanged(TaskStatus status, string? message)
        {
            var subsystemStatus = status switch
            {
                TaskStatus.RanToCompletion => SystemStatus.Finished,
                TaskStatus.Canceled => SystemStatus.Canceled,
                TaskStatus.Faulted => SystemStatus.Faulted,
                _ => SystemStatus.Active
            };

            StatusChanged?.Invoke(this, _status = subsystemStatus, message);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        private void OnStatusChanged(object sender, StreamStatus status, string message)
        {
            // TODO: Implement logic to respond to stream state changes.
        }

        /// <summary>
        /// Called when an interprocess frame is received.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="interprocessFrame">Interprocess frame.</param>
        private void OnFrameReceived(object sender, InterprocessFrame interprocessFrame)
        {
            var frame = ConfigureSystem(interprocessFrame);
            if (frame != null) _interprocessStream.TrySend(frame);
        }

        /// <summary>
        /// Called when a media frame is received.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="mediaFrame">Media frame.</param>
        /// <param name="metadataRequired">Indicates whether to include metadata.</param>
        private void OnFrameReceived(object sender, RawFrame mediaFrame, bool metadataRequired)
        {
            if (!(sender is IMediaStream mediaStream) || _endPoint == null)
                return;

            var result = CreateNetworkFrame(Name, mediaStream.Name, mediaStream.TotalFramesReceived,
                mediaFrame, metadataRequired);

            _networkStream.TrySend(result, _endPoint);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private InterprocessFrame? ConfigureSystem(InterprocessFrame input)
        {
            // TODO: Implement logic for executing system commands.
            return null;
        }

        /// <summary>
        /// Creates a new interprocess frame.
        /// </summary>
        /// <param name="systemName">System name.</param>
        /// <param name="streamName">Stream name.</param>
        /// <param name="message">Responce message.</param>
        /// <param name="result">Operation result.</param>
        /// <returns>A new interprocess frame.</returns>
        private static InterprocessFrame CreateInterprocessFrame(string systemName, string streamName,
            string message, bool? result = null)
        {
            var parameterDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"system", $"{systemName}"},
                {"stream", $"{streamName}"},
                {"message", $"{message}"},
                {"result", $"{result}"}
            };

            return new InterprocessFrame(parameterDictionary);
        }

        /// <summary>
        /// Creates a new network frame.
        /// </summary>
        /// <param name="systemName">System name.</param>
        /// <param name="streamName">Stream name.</param>
        /// <param name="totalFramesReceived">Total number of frames received by the media stream so far.</param>
        /// <param name="mediaFrame">Media frame.</param>
        /// <param name="metadataRequired">Indicates whether to include metadata.</param>
        /// <returns>A new network frame.</returns>
        private static NetworkFrame CreateNetworkFrame(string systemName, string streamName, int totalFramesReceived,
            RawFrame mediaFrame, bool metadataRequired)
        {
            byte interpretation = mediaFrame switch
            {
                RawAudioFrame _ => 1,
                RawVideoFrame _ => 2,
                _ => 0
            };

            return new NetworkFrame(
                (ulong) Chrono.GetUniqueTimestamp64(),
                (uint) totalFramesReceived,
                interpretation,
                systemName,
                streamName,
                CreateDataSegments(mediaFrame, metadataRequired));
        }

        /// <summary>
        /// Creates a new network endpoint.
        /// </summary>
        /// <param name="parameters">Configuration parameters.</param>
        /// <returns>A new network endpoint.</returns>
        private static IPEndPoint? CreateNetworkEndpoint(IReadOnlyDictionary<string, string> parameters)
        {
            return parameters.TryGetValue("address", out var address)
                   && parameters.TryGetValue("port", out var port)
                   && IPEndPoint.TryParse($"{address}:{port}", out var endPoint)
                ? endPoint
                : null;
        }

        /// <summary>
        /// Creates media frame data segments.
        /// </summary>
        /// <param name="mediaFrame">Media frame.</param>
        /// <param name="metadataRequired">Indicates whether to include metadata.</param>
        /// <returns>An array of data segments.</returns>
        private static ArraySegment<byte>[] CreateDataSegments(RawFrame mediaFrame, bool metadataRequired)
        {
            if (!metadataRequired) return new[] {new byte[] {0}, mediaFrame.FrameSegment};

            var codecName = mediaFrame switch
            {
                RawAACFrame _ => "AAC",
                RawG711AFrame _ => "G711A",
                RawG711UFrame _ => "G711U",
                RawG726Frame _ => "G726",
                RawPCMFrame _ => "PCM",
                RawH264IFrame _ => "H264",
                RawH264PFrame _ => "H264",
                RawJpegFrame _ => "MJPEG",
                _ => string.Empty
            };

            var bitsPerCodedUnit = mediaFrame switch
            {
                RawG726Frame rawG726Frame => rawG726Frame.BitsPerCodedSample,
                _ => 0
            };

            var configSegment = mediaFrame switch
            {
                RawAACFrame rawAacFrame => rawAacFrame.ConfigSegment,
                RawH264IFrame rawH264IFrame => rawH264IFrame.SpsPpsSegment,
                _ => default
            };

            var codecBytes = Encoding.UTF8.GetBytes(codecName);
            Array.Resize(ref codecBytes, 10);

            var metaSegment = new byte[19];
            using var stream = new MemoryStream(metaSegment);
            using var writer = new EndianBinaryWriter(stream);

            writer.Write((byte) 1);
            writer.Write(codecBytes);
            writer.Write(bitsPerCodedUnit);
            writer.Write(configSegment.Count);

            return configSegment.Count > 0
                ? new[] {metaSegment, configSegment, mediaFrame.FrameSegment}
                : new[] {metaSegment, mediaFrame.FrameSegment};
        }

        /// <summary>
        /// Creates a new media stream.
        /// </summary>
        /// <param name="parameters">Configuration parameters.</param>
        /// <returns>A new media stream.</returns>
        private static IMediaStream? CreateMediaStream(IReadOnlyDictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("name", out var name)
                || !parameters.TryGetValue("url", out var uri)
                || !Uri.TryCreate(uri, UriKind.Absolute, out var connectionUri))
                return null;

            parameters.TryGetValue("tracks", out var tracks);
            parameters.TryGetValue("transport", out var transport);
            parameters.TryGetValue("user", out var user);
            parameters.TryGetValue("password", out var password);
            parameters.TryGetValue("agent", out var agent);
            parameters.TryGetValue("connect_timeout", out var connectTimeout);
            parameters.TryGetValue("receive_timeout", out var receiveTimeout);
            parameters.TryGetValue("cancel_timeout", out var cancelTimeout);
            parameters.TryGetValue("metadata_frequency", out var metadataFrequency);
            parameters.TryGetValue("retry_delay", out var retryDelay);
            parameters.TryGetValue("number_of_attempts", out var numberOfAttempts);

            var networkCredential = string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password)
                ? null
                : new NetworkCredential(user, password);

            var connectionParameters = networkCredential == null
                ? new ConnectionParameters(connectionUri)
                : new ConnectionParameters(connectionUri, networkCredential);

            connectionParameters.UserAgent = string.IsNullOrEmpty(agent)
                ? connectionParameters.UserAgent
                : agent;

            connectionParameters.RequiredTracks = tracks?.ToLowerInvariant() switch
            {
                "all" => RequiredTracks.All,
                "audio" => RequiredTracks.Audio,
                "video" => RequiredTracks.Video,
                _ => connectionParameters.RequiredTracks
            };

            connectionParameters.RtpTransport = transport?.ToLowerInvariant() switch
            {
                "tcp" => RtpTransportProtocol.TCP,
                "udp" => RtpTransportProtocol.UDP,
                _ => connectionParameters.RtpTransport
            };

            if (double.TryParse(connectTimeout, out var doubleParseResult))
                connectionParameters.ConnectTimeout = TimeSpan.FromSeconds(doubleParseResult);

            if (double.TryParse(receiveTimeout, out doubleParseResult))
                connectionParameters.ConnectTimeout = TimeSpan.FromSeconds(doubleParseResult);

            if (double.TryParse(cancelTimeout, out doubleParseResult))
                connectionParameters.ConnectTimeout = TimeSpan.FromSeconds(doubleParseResult);

            TimeSpan? metadataFrequencyResult = null;
            if (double.TryParse(metadataFrequency, out doubleParseResult))
                metadataFrequencyResult = TimeSpan.FromSeconds(doubleParseResult);

            TimeSpan? retryDelayResult = null;
            if (double.TryParse(retryDelay, out doubleParseResult))
                retryDelayResult = TimeSpan.FromSeconds(doubleParseResult);

            int? numberOfAttemptsResult = null;
            if (int.TryParse(numberOfAttempts, out var intParseResult))
                numberOfAttemptsResult = intParseResult;

            return new MediaStream(name, connectionParameters, metadataFrequencyResult, retryDelayResult,
                numberOfAttemptsResult);
        }
    }
}
