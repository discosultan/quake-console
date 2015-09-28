using System;
using Microsoft.Xna.Framework;

namespace Sandbox
{
    public static class Utilities
    {
        private static readonly Random _rnd = new Random();

        public static Color RandomColor() => new Color(
            (float) _rnd.NextDouble(),
            (float) _rnd.NextDouble(),
            (float) _rnd.NextDouble(),
            1.0f);

        public static float Random() => (float)_rnd.NextDouble();

        public static int RandomInt(int maxValue) => _rnd.Next(maxValue);
    }
}
