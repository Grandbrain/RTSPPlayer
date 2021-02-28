namespace RTSPPlayerServer.Service.Systems
{
    /// <summary>
    /// An enumeration that describes system status.
    /// </summary>
    public enum SystemStatus
    {
        /// <summary>
        /// The system is active.
        /// </summary>
        Active,

        /// <summary>
        /// The system has finished.
        /// </summary>
        Finished,

        /// <summary>
        /// The system has been canceled.
        /// </summary>
        Canceled,

        /// <summary>
        /// The system has faulted.
        /// </summary>
        Faulted
    }
}
