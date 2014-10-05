using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Paradox.Input;

namespace Varus.Paradox.Console
{
    public static class Utilities
    {
        private static readonly Keys[] KeysEnumValues = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToArray();
        //public static void GetDownKeys(this InputManager input, List<Keys> pressedKeys)
        //{
        //    pressedKeys.Clear();
        //    input.
        //    pressedKeys.AddRange(KeysEnumValues.Where(input.IsKeyDown));
        //}
    }
}
