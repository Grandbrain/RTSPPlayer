using RTSPPlayerServer.Serializers.Interprocess;
using RTSPPlayerServer.Serializers.Network;
using RTSPPlayerServer.Servers.Media;
using RTSPPlayerServer.Streams.Interprocess;
using RTSPPlayerServer.Streams.Network;

namespace RTSPPlayerServer
{
    /// <summary>
    /// A class that provides program entry point implementation.
    /// </summary>
    internal static class Startup
    {
        /// <summary>
        /// Starts the program.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        private static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                Help.PrintShortHelp();
            }
            else if (args.Length == 1 && (args[0] == "--help" || args[0] == "-h"))
            {
                Help.PrintDetailedHelp();
            }
            else
            {
                INetworkSerializer networkSerializer = new NetworkSerializer();
                IInterprocessSerializer interprocessSerializer = new InterprocessSerializer();

                INetworkStream networkStream = new NetworkStream(networkSerializer);
                IInterprocessStream interprocessStream = new InterprocessStream(interprocessSerializer);

                IMediaServer mediaServer = new MediaServer(networkStream, interprocessStream);
                mediaServer.Name = args[0];
                mediaServer.AudioBaseName = args.Length > 1 ? args[1] : string.Empty;
                mediaServer.VideoBaseName = args.Length > 2 ? args[2] : string.Empty;
                mediaServer.Start();
                mediaServer.Wait();
            }
        }
    }
}
