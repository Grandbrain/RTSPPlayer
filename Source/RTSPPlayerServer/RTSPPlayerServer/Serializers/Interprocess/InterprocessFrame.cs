using System.Collections.Generic;

namespace RTSPPlayerServer.Serializers.Interprocess
{
    /// <summary>
    /// A class that defines interprocess frame.
    /// </summary>
    internal class InterprocessFrame
    {
        /// <summary>
        /// Dictionary of string parameters.
        /// </summary>
        public Dictionary<string, string> ParameterDictionary { get; set; }
    }
}
