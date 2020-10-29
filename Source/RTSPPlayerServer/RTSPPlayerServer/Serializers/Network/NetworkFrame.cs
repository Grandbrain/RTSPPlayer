using System;

namespace RTSPPlayerServer.Serializers.Network
{
    /// <summary>
    /// A class that defines a network frame.
    /// </summary>
    public class NetworkFrame
    {
        /// <summary>
        /// Frame identifier.
        /// </summary>
        public ulong Id { get; }

        /// <summary>
        /// Frame number.
        /// </summary>
        public uint Number { get; }

        /// <summary>
        /// Frame interpretation.
        /// </summary>
        public byte Interpretation { get; }

        /// <summary>
        /// Sender task identifier.
        /// </summary>
        public string Task { get; }

        /// <summary>
        /// Information flow identifier.
        /// </summary>
        public string Flow { get; }

        /// <summary>
        /// Frame data segments.
        /// </summary>
        public ArraySegment<byte>[] DataSegments { get; }

        /// <summary>
        /// Initializes a network frame with the specified parameters.
        /// </summary>
        /// <param name="id">Frame identifier.</param>
        /// <param name="number">Frame number.</param>
        /// <param name="interpretation">Frame interpretation.</param>
        /// <param name="task">Sender task identifier.</param>
        /// <param name="flow">Information flow identifier.</param>
        /// <param name="dataSegments">Frame data segments.</param>
        public NetworkFrame(ulong id, uint number, byte interpretation, string task, string flow,
            ArraySegment<byte>[] dataSegments)
        {
            Id = id;
            Number = number;
            Interpretation = interpretation;
            Task = task;
            Flow = flow;
            DataSegments = dataSegments;
        }
    }
}
