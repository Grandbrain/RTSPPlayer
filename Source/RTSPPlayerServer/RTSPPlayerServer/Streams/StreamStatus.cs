namespace RTSPPlayerServer.Streams
{
    /// <summary>
    /// An enumeration that describes stream status.
    /// </summary>
    public enum StreamStatus
    {
        /// <summary>
        /// The stream is active.
        /// </summary>
        Active,

        /// <summary>
        /// The stream has finished.
        /// </summary>
        Finished,

        /// <summary>
        /// The stream has been canceled.
        /// </summary>
        Canceled,

        /// <summary>
        /// The flow has faulted.
        /// </summary>
        Faulted
    }
}
