using System;
using System.Collections.Generic;

namespace Varus.Paradox.Console.CustomInterpreter.Utilities
{
    internal static class Utilities
    {
        /// <summary>
        /// An <see cref="IEnumerable{T}"/> extension method that searches for the first match and returns its index.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="source">Input enumerable to work on.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The index of the first element matching.</returns>
        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int index = 0;
            foreach (T item in source)
            {
                if (predicate(item))
                    return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        /// Execute an action for each item of the given enumerable.
        /// </summary>
        /// <typeparam name="T">Type of the item value in the enumerable.</typeparam>
        /// <param name="source">Input enumerable to work on.</param>
        /// <param name="action">Action performed for each item in the enumerable.</param>
        /// <remarks>This extension method do not yield. It acts just like a foreach statement.</remarks>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
                action(item);
        }
    }
}
