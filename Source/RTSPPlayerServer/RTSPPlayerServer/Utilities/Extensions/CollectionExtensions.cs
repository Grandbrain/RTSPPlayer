using System;
using System.Collections.Generic;

namespace RTSPPlayerServer.Utilities.Extensions
{
    /// <summary>
    /// An utility class that provides various extension methods.
    /// </summary>
    internal static class CollectionExtensions
    {
        /// <summary>
        /// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="source">The <see cref="IEnumerable{T}"/> collection.</param>
        /// <param name="action">The <see cref="Action{T}"/> delegate to perform on each element of the collection.
        /// </param>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach(var item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// Gets the first element of the <see cref="IEnumerable{T}"/> that satisfies a condition or a default value
        /// if no such element is found.
        /// </summary>
        /// <param name="source">An <see cref="IEnumerable{T}"/> to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="value">
        /// When this method returns, contains the <c>default(T)</c> if <c>source</c> is empty or if no element passes
        /// the test specified by <c>predicate</c>; otherwise, the first element in <c>source</c> that passes the test
        /// specified by <c>predicate</c>.
        /// </param>
        /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
        /// <returns><c>true</c> if <c>source</c> contains at least one element; <c>false</c> otherwise.</returns>
        public static bool TryFirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate,
            out TSource value)
        {
            foreach (var element in source)
            {
                if (!predicate(element)) continue;
                value = element;
                return true;
            }
            
            value = default;
            return false;
        }

        /// <summary>
        /// Adds a new or updates an existing element in the <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> collection.</param>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key)) dictionary[key] = value;
            else dictionary.Add(key, value);
        }
        
        /// <summary>
        /// Adds a new or updates an existing element in the <see cref="Dictionary{TKey,TValue}"/>.
        /// </summary>
        /// <param name="dictionary">The <see cref="Dictionary{TKey,TValue}"/> collection.</param>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            AddOrUpdate(dictionary as IDictionary<TKey, TValue>, key, value);
        }
    }
}
