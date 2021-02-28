using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSPPlayerServer.Service.Base.Extensions;

namespace RTSPPlayerServer.Service.Serializers.Interprocess
{
    /// <summary>
    /// A class that provides an interprocess serializer implementation.
    /// </summary>
    public class InterprocessSerializer : IInterprocessSerializer
    {
        /// <summary>
        /// A padding character in Base64 strings.
        /// </summary>
        private static char PaddingCharacter => '=';

        /// <summary>
        /// A character to separate keys and values in pairs.
        /// </summary>
        private static char PairCharacter => '=';

        /// <summary>
        /// A character to separate data records.
        /// </summary>
        private static char DelimiterCharacter => ' ';

        /// <summary>
        /// Serializes the interprocess frame into a string message.
        /// </summary>
        /// <param name="interprocessFrame">Interprocess frame.</param>
        /// <returns>Serialized message.</returns>
        public string Serialize(InterprocessFrame interprocessFrame)
        {
            return interprocessFrame.ParameterDictionary.Aggregate(
                string.Empty,
                (current, parameter) =>
                    current
                    + DelimiterCharacter
                    + $"{EncodeBase64(parameter.Key.ToLowerInvariant())}"
                    + PairCharacter
                    + $"{EncodeBase64(parameter.Value)}"

            ).ToLower();
        }

        /// <summary>
        /// Deserializes the string message into an interprocess frame.
        /// </summary>
        /// <param name="message">Serialized message.</param>
        /// <returns>Interprocess frame.</returns>
        public InterprocessFrame Deserialize(string message) =>
            new InterprocessFrame(ParseMessage(message));

        /// <summary>
        /// Parses the string message into the key-value parameter dictionary.
        /// </summary>
        /// <param name="message">Serialized message.</param>
        /// <returns>Parameter dictionary.</returns>
        private static Dictionary<string, string> ParseMessage(string message)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var pairs = message.Split(DelimiterCharacter, StringSplitOptions.RemoveEmptyEntries);
            if (!pairs.Any()) return dictionary;

            foreach (var pair in pairs)
            {
                var index = pair.IndexOf(PairCharacter);
                if (index <= 0 || index >= pair.Length - 1) continue;

                var key = DecodeBase64(pair.Substring(0, index).Trim());
                var value = DecodeBase64(pair.Substring(index + 1).Trim());
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) continue;

                dictionary.AddOrUpdate(key.ToLowerInvariant(), value);
            }

            return dictionary;
        }

        /// <summary>
        /// Encodes a regular string to Base64 format.
        /// </summary>
        /// <param name="data">Regular string.</param>
        /// <returns>Encoded Base64 string.</returns>
        private static string? EncodeBase64(string? data)
        {
            if (data == null) return null;

            var bytes = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(bytes).TrimEnd(PaddingCharacter);
        }

        /// <summary>
        /// Decodes a Base64 string to a regular string.
        /// </summary>
        /// <param name="data">Base64 string.</param>
        /// <returns>Decoded regular string.</returns>
        private static string? DecodeBase64(string? data)
        {
            if (data == null) return null;

            var length = (data.Length % 4) switch
            {
                2 => 2,
                3 => 1,
                _ => 0
            } + data.Length;

            var bytes = Convert.FromBase64String(data.PadRight(length, PaddingCharacter));
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
