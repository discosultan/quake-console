using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace QuakeConsole.Samples.Sandbox
{
    public static class Utilities
    {
        private static readonly CultureInfo DecimalParsingInfo = CultureInfo.InvariantCulture;
        private static readonly Random _rnd = new Random();

        public static Color RandomColor() => new Color(
            (float) _rnd.NextDouble(),
            (float) _rnd.NextDouble(),
            (float) _rnd.NextDouble(),
            1.0f);

        public static float Random() => (float)_rnd.NextDouble();

        public static int RandomInt(int maxValue) => _rnd.Next(maxValue);

        public static Vector3 ToVector3(this string[] args, int offset = 0)
        {
            ReplaceCommasWithDots(args);

            if (args.Length == 1)            
                return new Vector3(float.Parse(args[0 + offset], DecimalParsingInfo));
            
            return new Vector3(
                float.Parse(args[0 + offset], DecimalParsingInfo),
                float.Parse(args[1 + offset], DecimalParsingInfo),
                float.Parse(args[2 + offset], DecimalParsingInfo));
        }

        private static void ReplaceCommasWithDots(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
                args[i] = args[i].Replace(',', '.');
        }
    }
}
