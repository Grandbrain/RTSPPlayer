using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Timers;
using RtspClientSharp;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Audio;
using RtspClientSharp.RawFrames.Video;
using RTSPPlayerServer.Serializers.Interprocess;
using RTSPPlayerServer.Serializers.Network;
using RTSPPlayerServer.Streams.Interprocess;
using RTSPPlayerServer.Streams.Media;
using RTSPPlayerServer.Streams.Network;
using RTSPPlayerServer.Utilities;
using RTSPPlayerServer.Utilities.Extensions;

namespace RTSPPlayerServer.Servers.Media
{
    /// <summary>
    /// A class that provides media server implementation.
    /// </summary>
    internal class MediaServer : IMediaServer
    {
        /// <summary>
        /// Dictionary of media streams.
        /// </summary>
        private readonly IDictionary<string, Tuple<IMediaStream, EndPoint>> _mediaStreams =
            new Dictionary<string, Tuple<IMediaStream, EndPoint>>();
        
        /// <summary>
        /// Telemetry timer.
        /// </summary>
        private readonly Timer _telemetryTimer = new Timer();

        /// <summary>
        /// Network stream.
        /// </summary>
        private readonly INetworkStream _networkStream;

        /// <summary>
        /// Interprocess stream.
        /// </summary>
        private readonly IInterprocessStream _interprocessStream;

        /// <summary>
        /// Server name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The base name of the audio frame stream.
        /// </summary>
        public string AudioBaseName { get; set; }
        
        /// <summary>
        /// The base name of the video frame stream.
        /// </summary>
        public string VideoBaseName { get; set; }

        /// <summary>
        /// Constructs a media server with the specified network and interprocess streams.
        /// </summary>
        /// /// <param name="networkStream">Network stream.</param>
        /// <param name="interprocessStream">Interprocess stream.</param>
        /// <param name="telemetryInterval">Telemetry interval.</param>
        public MediaServer(INetworkStream networkStream, IInterprocessStream interprocessStream,
            double telemetryInterval)
        {
            _networkStream = networkStream ?? throw new ArgumentNullException(nameof(networkStream));
            
            _interprocessStream = interprocessStream ?? throw new ArgumentNullException(nameof(interprocessStream));
            _interprocessStream.FrameReceived += OnMessageReceived;
            
            _telemetryTimer.Interval = telemetryInterval;
            _telemetryTimer.Elapsed += OnTelemetryTimeout;
        }

        /// <summary>
        /// Starts the media server.
        /// </summary>
        public void Start()
        {
            _networkStream?.Start();
            _interprocessStream?.Start();
            _telemetryTimer?.Start();
        }

        /// <summary>
        /// Stops the media server.
        /// </summary>
        public void Stop()
        {
            _mediaStreams?.Values.ForEach(tuple => tuple?.Item1?.Stop());
            _networkStream?.Stop();
            _interprocessStream?.Stop();
            _telemetryTimer?.Stop();
        }

        /// <summary>
        /// Waits until the media server finishes work.
        /// </summary>
        public void Wait()
        {
            _interprocessStream?.Wait();
            _networkStream?.Wait();
            _mediaStreams?.Values.ForEach(tuple => tuple?.Item1?.Wait());
        }

        /// <summary>
        /// Called upon receipt of a media frame.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="rawFrame">Raw frame.</param>
        /// <param name="metadataRequired">Indicates whether to include metadata.</param>
        private void OnFrameReceived(object sender, RawFrame rawFrame, bool metadataRequired)
        {
            if (!_mediaStreams.TryFirstOrDefault(e => ReferenceEquals(e.Value?.Item1, sender), out var entry) ||
                !(rawFrame is RawAudioFrame || rawFrame is RawVideoFrame)) return;
            
            var (mediaStream, endPoint) = entry.Value;

            var networkFrame = new NetworkFrame
            {
                Id = ChronoUtilities.TimestampMicroseconds32(),
                Number = (ushort) mediaStream.TotalFramesReceived,
                Interpretation = (byte) (rawFrame is RawAudioFrame ? 1 : 2),
                Time = 0,
                Priority = 10,
                Task = Name,
                Flow = (rawFrame is RawAudioFrame ? AudioBaseName : VideoBaseName) + entry.Key,
                DataSegments = GetDataSegments(rawFrame, metadataRequired)
            };
            
            if (_networkStream?.IsHealthy ?? false) 
                _networkStream?.TrySend(networkFrame, endPoint);
        }

        /// <summary>
        /// Called upon receipt of an interprocess frame.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="interprocessFrame">Interprocess frame.</param>
        private void OnMessageReceived(object sender, InterprocessFrame interprocessFrame)
        {
            if (interprocessFrame?.ParameterDictionary != null &&
                interprocessFrame.ParameterDictionary.TryGetValue("command", out var command))
                DispatchCommand(command, interprocessFrame.ParameterDictionary);
        }

        /// <summary>
        /// Called when the telemetry interval elapses.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="elapsedEventArgs">Elapsed event arguments.</param>
        private void OnTelemetryTimeout(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var parameterDictionary = new Dictionary<string, string>();

            foreach (var (key, value) in _mediaStreams)
            {
                var active = value?.Item1?.IsActive switch
                {
                    true => "yes",
                    _ => "no"
                };

                var healthy = value?.Item1?.IsHealthy switch
                {
                    true => "yes",
                    _ => "no"
                };

                var tracks = value?.Item1?.ConnectionParameters?.RequiredTracks switch
                {
                    RequiredTracks.Video => "video",
                    RequiredTracks.Audio => "audio",
                    RequiredTracks.All => "all",
                    _ => "none"
                };
                
                parameterDictionary.AddOrUpdate(key, $"{active}|{healthy}|{tracks}");
            }

            if (_interprocessStream?.IsHealthy ?? false)
                _interprocessStream?.TrySend(new InterprocessFrame {ParameterDictionary = parameterDictionary});
        }

        /// <summary>
        /// Returns media frame data segments.
        /// </summary>
        /// <param name="rawFrame">Media frame.</param>
        /// <param name="metadataRequired">Indicates whether to include metadata.</param>
        /// <returns>Data segments array.</returns>
        private static ArraySegment<byte>[] GetDataSegments(RawFrame rawFrame, bool metadataRequired)
        {
            if (!metadataRequired) return new[] {new byte[] {0}, rawFrame.FrameSegment};
            
            var codecName = rawFrame switch
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

            var bitsPerCodedUnit = rawFrame switch
            {
                RawG726Frame rawG726Frame => rawG726Frame.BitsPerCodedSample,
                _ => 0
            };

            var configSegment = rawFrame switch
            {
                RawAACFrame rawAacFrame => rawAacFrame.ConfigSegment,
                RawH264IFrame rawH264IFrame => rawH264IFrame.SpsPpsSegment,
                _ => default
            };

            var codecBytes = Encoding.Default.GetBytes(codecName);
            Array.Resize(ref codecBytes, 10);

            var metaSegment = new byte[19];
            using var stream = new MemoryStream(metaSegment);
            using var writer = new BinaryWriter(stream);

            writer.Write((byte) 1);
            writer.Write(codecBytes);
            writer.Write(bitsPerCodedUnit);
            writer.Write(configSegment.Count);
                
            return configSegment.Count > 0
                ? new[] {metaSegment, configSegment, rawFrame.FrameSegment}
                : new[] {metaSegment, rawFrame.FrameSegment};

        }

        /// <summary>
        /// Dispatches a command to a specific action.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="parameters">Configuration parameters.</param>
        private void DispatchCommand(string command, IDictionary<string, string> parameters)
        {
            try
            {
                switch (command)
                {
                    case "add":
                        AddMediaStream(parameters);
                        break;
                    case "remove":
                        RemoveMediaStream(parameters);
                        break;
                    case "start":
                        StartMediaStream(parameters);
                        break;
                    case "stop":
                        StopMediaStream(parameters);
                        break;
                    case "set":
                        SetMediaStream(parameters);
                        break;
                    case "close":
                        Stop();
                        break;
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Adds a new media stream.
        /// </summary>
        /// <param name="parameters">Configuration parameters.</param>
        private void AddMediaStream(IDictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("name", out var name) ||
                !parameters.TryGetValue("url", out var uri) ||
                !Uri.TryCreate(uri, UriKind.Absolute, out var connectionUri))
                return;

            parameters.TryGetValue("media", out var media);
            parameters.TryGetValue("transport", out var transport);
            parameters.TryGetValue("user", out var user);
            parameters.TryGetValue("password", out var password);
            parameters.TryGetValue("agent", out var agent);
            parameters.TryGetValue("connect_timeout", out var connectTimeout);
            parameters.TryGetValue("receive_timeout", out var receiveTimeout);
            parameters.TryGetValue("cancel_timeout", out var cancelTimeout);
            parameters.TryGetValue("retry_delay", out var retryDelay);

            var networkCredential = string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password)
                ? null
                : new NetworkCredential(user, password);

            var connectionParameters = networkCredential == null
                ? new ConnectionParameters(connectionUri)
                : new ConnectionParameters(connectionUri, networkCredential);

            connectionParameters.RequiredTracks = media switch
            {
                "all" => RequiredTracks.All,
                "audio" => RequiredTracks.Audio,
                "video" => RequiredTracks.Video,
                _ => connectionParameters.RequiredTracks
            };

            connectionParameters.RtpTransport = transport switch
            {
                "tcp" => RtpTransportProtocol.TCP,
                "udp" => RtpTransportProtocol.UDP,
                _ => connectionParameters.RtpTransport
            };

            connectionParameters.UserAgent = agent switch
            {
                null => connectionParameters.UserAgent,
                "" => connectionParameters.UserAgent,
                _ => agent
            };

            if (double.TryParse(connectTimeout, out var result))
                connectionParameters.ConnectTimeout = TimeSpan.FromSeconds(result);

            if (double.TryParse(receiveTimeout, out result))
                connectionParameters.ConnectTimeout = TimeSpan.FromSeconds(result);

            if (double.TryParse(cancelTimeout, out result))
                connectionParameters.ConnectTimeout = TimeSpan.FromSeconds(result);

            TimeSpan? timeSpan = null;
            if (double.TryParse(retryDelay, out result)) timeSpan = TimeSpan.FromSeconds(result);

            IMediaStream mediaStream = new MediaStream(connectionParameters, timeSpan);
            mediaStream.FrameReceived += OnFrameReceived;

            RemoveMediaStream(parameters);
            _mediaStreams.AddOrUpdate(name, Tuple.Create(mediaStream, null as EndPoint));
        }

        /// <summary>
        /// Removes the media stream.
        /// </summary>
        /// <param name="parameters">Configuration parameters.</param>
        private void RemoveMediaStream(IDictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("name", out var name)) 
                return;

            if (_mediaStreams.ContainsKey(name))
                _mediaStreams[name]?.Item1?.Stop();

            _mediaStreams.Remove(name);
            if (_mediaStreams.ContainsKey(name)) _mediaStreams[name] = null;
        }

        /// <summary>
        /// Starts the media stream.
        /// </summary>
        /// <param name="parameters">Configuration parameters.</param>
        private void StartMediaStream(IDictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("name", out var name) ||
                !_mediaStreams.TryGetValue(name, out var tuple) ||
                tuple?.Item1 == null ||
                tuple.Item2 == null) 
                return;
            
            tuple.Item1.Start();
        }

        /// <summary>
        /// Stops the media stream.
        /// </summary>
        /// <param name="parameters">Configuration parameters.</param>
        private void StopMediaStream(IDictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("name", out var name) ||
                !_mediaStreams.TryGetValue(name, out var tuple) ||
                tuple?.Item1 == null) 
                return;

            tuple.Item1.Stop();
        }

        /// <summary>
        /// Sets the parameters for the media stream.
        /// </summary>
        /// <param name="parameters">Configuration parameters.</param>
        private void SetMediaStream(IDictionary<string, string> parameters)
        {
            if (!parameters.TryGetValue("name", out var name) ||
                !_mediaStreams.TryGetValue(name, out var tuple) ||
                tuple?.Item1 == null) 
                return;
            
            var active = tuple.Item1.IsActive; 
            if (active) tuple.Item1.Stop();

            if (parameters.TryGetValue("address", out var address) &&
                parameters.TryGetValue("port", out var port) &&
                IPAddress.TryParse(address, out var ipAddress) &&
                int.TryParse(port, out var ipPort))
                tuple = Tuple.Create(tuple.Item1, new IPEndPoint(ipAddress, ipPort) as EndPoint);

            _mediaStreams.AddOrUpdate(name, tuple);
            if (active) tuple.Item1.Start();
        }
    }
}
