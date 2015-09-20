using System;
using System.Text;

namespace QuakeConsole.Utilities
{
    internal static class StringExtensions
    {
        public static bool IsEmptyOrWhitespace(this StringBuilder value)
        {
            if (value.Length == 0) return true;

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] != ' ') return false;
            }

            return true;
        }

        public static void ClearAndCopyTo(this StringBuilder from, StringBuilder to, int sourceIndex, int sourceLength)
        {
            to.Clear();
            for (int i = 0; i < sourceLength; i++)
            {
                to.Append(from[sourceIndex + i]);
            }
        }

        // Not thread safe!
        private static readonly string[] _array = new string[1];
        public static string[] Split(this string value, string separator, StringSplitOptions options)
        {
            _array[0] = separator;
            return value.Split(_array, options);
        }

#if MONOGAME
        public static string Substring(this StringBuilder builder, int startIndex)
        {
            return builder.ToString(startIndex, builder.Length - startIndex);
        }

        public static string Substring(this StringBuilder builder, int startIndex, int length)
        {
            return builder.ToString(startIndex, length);
        }
#endif
    }
}
