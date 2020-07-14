namespace RTSPPlayerServer.Servers.Media
{
    /// <summary>
    /// 
    /// </summary>
    internal interface IMediaServer
    {
        /// <summary>
        /// 
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string AudioBaseName { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        string VideoBaseName { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        void Start();

        /// <summary>
        /// 
        /// </summary>
        void Stop();

        /// <summary>
        /// 
        /// </summary>
        void Wait();
    }
}
