using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using Varus.Paradox.Console.Interpreters.Custom;

namespace Varus.Paradox.Console.Sample
{
    internal static class ArgumentHelper
    {
        private static readonly CultureInfo DecimalParsingInfo = CultureInfo.InvariantCulture;

        public static Vector2 ToVector2(this string[] args, int offset = 0)
        {
            ReplaceCommasWithDots(args);
            return new Vector2(
                float.Parse(args[0 + offset], DecimalParsingInfo),
                float.Parse(args[1 + offset], DecimalParsingInfo));
        }

        public static Vector3 ToVector3(this string[] args, int offset = 0)
        {
            ReplaceCommasWithDots(args);
            return new Vector3(
                float.Parse(args[0 + offset], DecimalParsingInfo),
                float.Parse(args[1 + offset], DecimalParsingInfo),
                float.Parse(args[2 + offset], DecimalParsingInfo));
        }

        public static Vector4 ToVector4(this string[] args, int offset = 0)
        {
            ReplaceCommasWithDots(args);
            return new Vector4(
                float.Parse(args[0 + offset], DecimalParsingInfo),
                float.Parse(args[1 + offset], DecimalParsingInfo),
                float.Parse(args[2 + offset], DecimalParsingInfo),
                float.Parse(args[3 + offset], DecimalParsingInfo));
        }

        public static float ToSingle(this string[] args, int offset = 0)
        {
            ReplaceCommasWithDots(args);
            return float.Parse(args[0 + offset], DecimalParsingInfo);
        }

        public static int ToInteger(this string[] args, int offset = 0)
        {
            return int.Parse(args[0 + offset]);
        }

        public static bool ToBoolean(this string[] args, int offset = 0)
        {
            return bool.Parse(args[0 + offset]);
        }

        public static Color4 ToColor4(this string[] args, int offset = 0)
        {
            if (ArgsContainsDecimal(args))
            {
                ReplaceCommasWithDots(args);
                return new Color4(
                    float.Parse(args[0 + offset], DecimalParsingInfo),
                    float.Parse(args[1 + offset], DecimalParsingInfo),
                    float.Parse(args[2 + offset], DecimalParsingInfo),
                    args.Length + offset >= 4 ? float.Parse(args[3 + offset], DecimalParsingInfo) : 1.0f);
            }

            var color = new Color(
                byte.Parse(args[0 + offset]),
                byte.Parse(args[1 + offset]),
                byte.Parse(args[2 + offset]),
                args.Length + offset >= 4 ? byte.Parse(args[3 + offset]) : byte.MaxValue);
            return color.ToColor4();
        }

        public static bool FailWhenLengthLessThan(this string[] args, int expectedLength, CommandResult result, string failMessage)
        {
            if (args.Length < expectedLength)
            {
                result.IsFaulted = true;
                result.Message = failMessage;
                return true;
            }
            return false;
        }

        private static bool ArgsContainsDecimal(IEnumerable<string> args)
        {
            return args.SelectMany(arg => arg).Any(letter => letter == ',' || letter == '.');
        }

        private static void ReplaceCommasWithDots(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                args[i] = args[i].Replace(',', '.');
            }
        }
    }
}
