using System;
using System.Collections.Generic;

namespace RTSPPlayerServer.Service.Base.Extensions
{
    /// <summary>
    /// An utility class that provides various extension methods for collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds a new or updates an existing element in the <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> collection.</param>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
            where TKey : notnull
        {
            if (dictionary.ContainsKey(key)) dictionary[key] = value;
            else dictionary.Add(key, value);
        }

        /// <summary>
        /// Gets the first element of the <see cref="IEnumerable{T}"/> that satisfies a condition or a default value
        /// if no such element is found.
        /// </summary>
        /// <param name="source">An <see cref="IEnumerable{T}"/> to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="value">
        /// When this method returns, contains the default value if <paramref name="source"/> is empty or if no element
        /// passes the test specified by <paramref name="predicate"/>; otherwise, the first element in
        /// <paramref name="source"/> that passes the test specified by <paramref name="predicate"/>.
        /// </param>
        /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
        /// <returns>
        /// <c>true</c> if <paramref name="source"/> contains at least one element;
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool TryFirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate,
            out TSource value)
        {
            foreach (var element in source)
            {
                if (!predicate(element)) continue;
                value = element;
                return true;
            }

            value = default!;
            return false;
        }
    }
}
