using System;
using System.Threading;

namespace RTSPPlayerServer.Service.Base.Utilities
{
    /// <summary>
    /// A utility class that provides methods for creating timestamps and identifiers.
    /// </summary>
    public static class Chrono
    {
        /// <summary>
        /// Latest timestamp in microseconds since Unix Epoch.
        /// </summary>
        private static long _latestTimestamp;

        /// <summary>
        /// Returns a 64-bit unique timestamp.
        /// </summary>
        /// <returns>A 64-bit unique timestamp.</returns>
        public static long GetUniqueTimestamp64()
        {
            long original, update;

            do
            {
                original = _latestTimestamp;
                update = Math.Max(original + 1, (DateTime.UtcNow - DateTime.UnixEpoch).Ticks / 10L);
            } while (Interlocked.CompareExchange(ref _latestTimestamp, update, original) != original);

            return update;
        }
    }
}
