using System.Collections.Generic;

namespace RTSPPlayerServer.Serializers.Interprocess
{
    /// <summary>
    /// A class that defines an interprocess frame.
    /// </summary>
    public class InterprocessFrame
    {
        /// <summary>
        /// Dictionary of string parameters.
        /// </summary>
        public Dictionary<string, string> ParameterDictionary { get; }

        /// <summary>
        /// Initializes an interprocess frame with the specified parameters.
        /// </summary>
        /// <param name="parameterDictionary"></param>
        public InterprocessFrame(Dictionary<string, string> parameterDictionary)
        {
            ParameterDictionary = parameterDictionary;
        }
    }
}
