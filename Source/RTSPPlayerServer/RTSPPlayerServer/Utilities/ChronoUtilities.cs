using System;

namespace RTSPPlayerServer.Utilities
{
    /// <summary>
    /// An utility class that provides methods for calculating timestamps.
    /// </summary>
    internal static class ChronoUtilities
    {
        /// <summary>
        /// Latest timestamp in microseconds.
        /// </summary>
        private static long _baseMicroseconds = DateTime.Now.Ticks / 10L;

        /// <summary>
        /// Generates a 32-bit timestamp from microseconds.
        /// </summary>
        /// <returns>A 32-bit timestamp from microseconds.</returns>
        public static uint TimestampMicroseconds32()
        {
            var currentMicroseconds = DateTime.Now.Ticks / 10L;

            _baseMicroseconds = currentMicroseconds <= _baseMicroseconds
                ? _baseMicroseconds + 1
                : currentMicroseconds;

            return (uint) _baseMicroseconds;
        }
    }
}
