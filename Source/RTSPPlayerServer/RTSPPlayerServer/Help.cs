using System;

namespace RTSPPlayerServer
{
    /// <summary>
    /// A class that provides help output implementation.
    /// </summary>
    internal static class Help
    {
        /// <summary>
        /// Application name.
        /// </summary>
        private static readonly string AppName = AppDomain.CurrentDomain.FriendlyName;

        /// <summary>
        /// Prints short help.
        /// </summary>
        public static void PrintShortHelp()
        {
            Console.WriteLine($"{AppName}: missing arguments");
            Console.WriteLine($"Try '{AppName} --help' for more information.");
        }

        /// <summary>
        /// Prints detailed help.
        /// </summary>
        public static void PrintDetailedHelp()
        {
            Console.WriteLine($"Usage: {AppName} SERVER_NAME [AUDIO_STREAM_BASENAME [VIDEO_STREAM_BASENAME [TELEMETRY_INTERVAL]]]");
            Console.WriteLine("\"SERVER_NAME\" - the name of the server that will also identify outgoing packets;");
            Console.WriteLine("\"AUDIO_STREAM_BASENAME\" - base name of the audio stream (optional);");
            Console.WriteLine("\"VIDEO_STREAM_BASENAME\" - base name of the video stream (optional).");
            Console.WriteLine("\"TELEMETRY_INTERVAL\" - telemetry interval in milliseconds (optional).");
            Console.WriteLine();

            Console.WriteLine("The server accepts commands through interprocess I/O in the following format:");
            Console.WriteLine("\"key1=value1 key2=value2 ... keyN=valueN\", where key/value pairs are string parameters.");
            Console.WriteLine();

            Console.WriteLine("To add a media stream, use the following pairs:");
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine($"| {"Key",17} | {"Value",44} | {"Presence",8} |");
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine($"| {"\"command\"",17} | {"\"add\"",44} | {"Required",8} |");
            Console.WriteLine($"| {"\"name\"",17} | {"Unique media stream name",44} | {"Required",8} |");
            Console.WriteLine($"| {"\"url\"",17} | {"Media absolute URL (e.g. \"rtsp://...\")",44} | {"Required",8} |");
            Console.WriteLine($"| {"\"media\"",17} | {"\"all\"|\"audio\"|\"video\"",44} | {"Optional",8} |");
            Console.WriteLine($"| {"\"transport\"",17} | {"\"tcp\"|\"udp\"",44} | {"Optional",8} |");
            Console.WriteLine($"| {"\"user\"",17} | {"Authentication username",44} | {"Optional",8} |");
            Console.WriteLine($"| {"\"password\"",17} | {"Authentication password",44} | {"Optional",8} |");
            Console.WriteLine($"| {"\"agent\"",17} | {"User agent name",44} | {"Optional",8} |");
            Console.WriteLine($"| {"\"connect_timeout\"",17} | {"Connection timeout (in seconds)",44} | {"Optional",8} |");
            Console.WriteLine($"| {"\"receive_timeout\"",17} | {"Receive timeout (in seconds)",44} | {"Optional",8} |");
            Console.WriteLine($"| {"\"cancel_timeout\"",17} | {"Cancel timeout (in seconds)",44} | {"Optional",8} |");
            Console.WriteLine($"| {"\"retry_delay\"",17} | {"Delay between failed operations (in seconds)",44} | {"Optional",8} |");
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine();

            Console.WriteLine("To remove the media stream, use the following pairs:");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine($"| {"Key",9} | {"Value",17} | {"Presence",8} |");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine($"| {"\"command\"",9} | {"\"remove\"",17} | {"Required",8} |");
            Console.WriteLine($"| {"\"name\"",9} | {"Media stream name",17} | {"Required",8} |");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine();

            Console.WriteLine("To start the media stream, use the following pairs:");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine($"| {"Key",9} | {"Value",17} | {"Presence",8} |");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine($"| {"\"command\"",9} | {"\"start\"",17} | {"Required",8} |");
            Console.WriteLine($"| {"\"name\"",9} | {"Media stream name",17} | {"Required",8} |");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine();

            Console.WriteLine("To stop the media stream, use the following pairs:");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine($"| {"Key",9} | {"Value",17} | {"Presence",8} |");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine($"| {"\"command\"",9} | {"\"stop\"",17} | {"Required",8} |");
            Console.WriteLine($"| {"\"name\"",9} | {"Media stream name",17} | {"Required",8} |");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine();
            
            Console.WriteLine("To set parameters for the media stream, use the following pairs:");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine($"| {"Key",9} | {"Value",34} | {"Presence",8} |");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine($"| {"\"command\"",9} | {"\"set\"",34} | {"Required",8} |");
            Console.WriteLine($"| {"\"name\"",9} | {"Media stream name",34} | {"Required",8} |");
            Console.WriteLine($"| {"\"address\"",9} | {"Network address (e.g. \"127.0.0.1\")",34} | {"Optional",8} |");
            Console.WriteLine($"| {"\"port\"",9} | {"Network port",34} | {"Optional",8} |");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine();
        }
    }
}
