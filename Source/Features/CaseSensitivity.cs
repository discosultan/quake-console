using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace QuakeConsole.Features
{
    internal class CaseSensitivity
    {
        private Console _console;

        public bool Enabled { get; set; } = true;

        public void LoadContent(Console console)
        {
            _console = console;
            CheckKeysToggled();
        }

        private bool _capsLockToggled;

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;

            switch (action)
            {
                case ConsoleAction.CapsLock:
                    CheckKeysToggled();
                    break;
            }
        }

        public string ProcessSymbol(Symbol symbol)
        {
            if (!Enabled)
                return symbol.Lowercase;

            List<Keys> uppercaseModifiers;
            _console.ActionDefinitions.BackwardTryGetValues(ConsoleAction.UppercaseModifier, out uppercaseModifiers);

            bool capsLockApplies = symbol.Lowercase.Length == 1 && char.IsLetter(symbol.Lowercase[0]) && _capsLockToggled;            
            bool uppercaseModifierApplies = uppercaseModifiers != null && uppercaseModifiers.Any(x => _console.ConsoleInput.Input.IsKeyDown(x));

            return capsLockApplies ^ uppercaseModifierApplies 
                ? symbol.Uppercase 
                : symbol.Lowercase;
        }

        private void CheckKeysToggled()
        {
            _capsLockToggled = _console.ConsoleInput.Input.IsKeyToggled(Keys.CapsLock);
            //_numLockToggled = _console.ConsoleInput.Input.IsKeyToggled(Keys.NumLock);
        }
    }
}
