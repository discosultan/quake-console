using System.Text;

namespace Varus.Paradox.Console.Utilities
{
    internal static class Utilities
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

        private static readonly string[] Array = new string[1];
        public static string[] AsArray(this string value)
        {
            Array[0] = value;
            return Array;
        }
    }
}
