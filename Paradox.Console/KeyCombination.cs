using SiliconStudio.Core.Extensions;
using SiliconStudio.Paradox.Input;
using System.Linq;

namespace Varus.Paradox.Console
{
    public class KeyCombination
    {
        private readonly Keys[] _keys;

        public KeyCombination(Keys[] keys)
        {
            _keys = keys;
        }

        //public bool AreAllPressed()
        //{
        //    _keys.All(x => x.);
        //}
    }
}
