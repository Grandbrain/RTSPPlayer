using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using RTSPPlayerServer.Base.Utilities;

namespace RTSPPlayerServer.Tests.Base.Utilities
{
    /// <summary>
    /// A class that contains tests for <see cref="Chrono"/> class.
    /// </summary>
    [TestFixture]
    public class ChronoTests
    {
        /// <summary>
        /// Tests the <see cref="Chrono.GetUniqueTimestamp64"/> method.
        /// </summary>
        /// <param name="numberOfTimestamps">Number of timestamps to generate.</param>
        /// <returns>Asynchronous task.</returns>
        [TestCase(1000, ExpectedResult = true)]
        public async Task<bool> TestGetUniqueTimestamp64(int numberOfTimestamps)
        {
            IEnumerable<long> GetUniqueTimestamps64()
            {
                return Enumerable.Range(1, numberOfTimestamps)
                    .Select(_ => Chrono.GetUniqueTimestamp64())
                    .ToList();
            }

            var result = await Task.WhenAll(
                Task.Run(GetUniqueTimestamps64),
                Task.Run(GetUniqueTimestamps64),
                Task.Run(GetUniqueTimestamps64));

            return result.GroupBy(item => item).All(group => group.Count() == 1);
        }
    }
}
