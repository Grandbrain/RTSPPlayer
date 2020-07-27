using System;
using System.Collections.Generic;
using System.Linq;

namespace RTSPPlayerServer.Serializers.Interprocess
{
    internal class InterprocessSerializer : IInterprocessSerializer
    {
        /// <summary>
        /// Serializes the interprocess frame into a string message.
        /// </summary>
        /// <param name="interprocessFrame">Interprocess frame.</param>
        /// <returns>Serialized message.</returns>
        public string Serialize(InterprocessFrame interprocessFrame)
        {
            return interprocessFrame?.ParameterDictionary?.Aggregate(string.Empty,
                (current, parameter) => current + " " + $"{parameter.Key}={parameter.Value}").ToLower() ?? string.Empty;
        }

        /// <summary>
        /// Deserializes the string message into an interprocess frame.
        /// </summary>
        /// <param name="message">Serialized message.</param>
        /// <returns>Interprocess frame.</returns>
        public InterprocessFrame Deserialize(string message)
        {
            return new InterprocessFrame {ParameterDictionary = ParseMessage(message)};
        }
        
        /// <summary>
        /// Parses the string message into the key-value parameter dictionary.
        /// </summary>
        /// <param name="message">Serialized message.</param>
        /// <returns>Parameter dictionary.</returns>
        private static Dictionary<string, string> ParseMessage(string message)
        {
            var dictionary = new Dictionary<string, string>();
            var pairs = message?.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (!pairs?.Any() ?? true) return dictionary;

            foreach (var pair in pairs)
            {
                var index = pair.IndexOf('=');
                if (index <= 0 || index >= pair.Length - 1) continue;

                var key = pair.Substring(0, index).Trim();
                var value = pair.Substring(index + 1).Trim();
                
                if (!string.IsNullOrEmpty(key))
                    dictionary.TryAdd(key, value);
            }

            return dictionary;
        }
    }
}
