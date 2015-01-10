using System;
using System.Text;

namespace Varus.Paradox.Console.Utilities
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

        public static string[] Split(this string value, string separator, StringSplitOptions options)
        {
            return value.Split(separator.AsArray(), options);
        }

        // Not thread safe!.
        private static readonly string[] Array = new string[1];
        private static string[] AsArray(this string value)
        {
            Array[0] = value;
            return Array;
        }
    }
}
