using System.Collections.Generic;
using NUnit.Framework;
using RTSPPlayerServer.Base.Extensions;

namespace RTSPPlayerServer.Tests.Base.Extensions
{
    /// <summary>
    /// A class that contains tests for <see cref="RTSPPlayerServer.Base.Extensions.CollectionExtensions"/> class.
    /// </summary>
    [TestFixture]
    public class CollectionExtensionsTests
    {
        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return
                new TestCaseData(new List<(int Key, int Value)> {(1, 1), (2, 2), (3, 3), (4, 4), (5, 5)})
                    .Returns(new[] {1, 2, 3, 4, 5});
        }

        /// <summary>
        /// Tests the <see cref="RTSPPlayerServer.Base.Extensions.CollectionExtensions.AddOrUpdate{TKey,TValue}"/>
        /// </summary>
        /// <param name="items">List of dictionary items.</param>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <returns>An array of dictionary values.</returns>
        [TestCaseSource(nameof(TestCases))]
        public IEnumerable<TValue> TestAddOrUpdate<TKey, TValue>(IEnumerable<(TKey Key, TValue Value)> items)
            where TKey : notnull
        {
            var dictionary = new Dictionary<TKey, TValue>();
            foreach (var (key, value) in items) dictionary.AddOrUpdate(key, value);
            return dictionary.Values;
        }
    }
}
