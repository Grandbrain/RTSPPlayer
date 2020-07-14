using System;

namespace RTSPPlayerServer.Serializers.Network
{
    /// <summary>
    /// A class that defines a network frame.
    /// </summary>
    internal class NetworkFrame
    {
        /// <summary>
        /// Frame identifier.
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Frame number.
        /// </summary>
        public ushort Number { get; set; }
        
        /// <summary>
        /// Frame interpretation.
        /// </summary>
        public byte Interpretation { get; set; }

        /// <summary>
        /// Frame processing time.
        /// </summary>
        public ushort Time { get; set; }

        /// <summary>
        /// Frame priority.
        /// </summary>
        public byte Priority { get; set; }

        /// <summary>
        /// Sender task identifier.
        /// </summary>
        public string Task { get; set; }

        /// <summary>
        /// Information flow identifier.
        /// </summary>
        public string Flow { get; set; }

        /// <summary>
        /// Frame data segments.
        /// </summary>
        public ArraySegment<byte>[] DataSegments { get; set; }
    }
}
