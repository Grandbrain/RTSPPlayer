using RTSPPlayerServer.Service.Serializers.Interprocess;
using RTSPPlayerServer.Service.Serializers.Network;
using RTSPPlayerServer.Service.Streams.Interprocess;
using RTSPPlayerServer.Service.Streams.Network;
using RTSPPlayerServer.Service.Systems.Media;

namespace RTSPPlayerServer.Service
{
    /// <summary>
    /// A class that provides the program entry point implementation.
    /// </summary>
    public static class Startup
    {
        /// <summary>
        /// Default interprocess stream name.
        /// </summary>
        private static string DefaultInterprocessStreamName => "interprocess";

        /// <summary>
        /// Default network stream name.
        /// </summary>
        private static string DefaultNetworkStreamName => "network";

        /// <summary>
        /// Default media system name.
        /// </summary>
        private static string DefaultMediaSystemName => "media";

        /// <summary>
        /// Starts the program.
        /// </summary>
        private static void Main()
        {
            IInterprocessSerializer interprocessSerializer = new InterprocessSerializer();
            INetworkSerializer networkSerializer = new NetworkSerializer();

            IInterprocessStream interprocessStream =
                new InterprocessStream(DefaultInterprocessStreamName, interprocessSerializer);

            INetworkStream networkStream =
                new NetworkStream(DefaultNetworkStreamName, networkSerializer);

            IMediaSystem mediaSystem =
                new MediaSystem(DefaultMediaSystemName, interprocessStream, networkStream);

            interprocessStream.Start();
            networkStream.Start();

            mediaSystem.Start();
            mediaSystem.WaitForFinished();

            interprocessStream.Stop();
            networkStream.Stop();

            interprocessStream.WaitForFinished();
            networkStream.WaitForFinished();
        }
    }
}
