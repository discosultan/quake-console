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
            
            bool capsLockApplies = symbol.Lowercase.Length == 1 && char.IsLetter(symbol.Lowercase[0]) && _capsLockToggled;
            bool uppercaseModifierApplies = _console.ActionDefinitions.AreModifiersAppliedForAction(ConsoleAction.UppercaseModifier);

            return capsLockApplies ^ uppercaseModifierApplies 
                ? symbol.Uppercase 
                : symbol.Lowercase;
        }

        private void CheckKeysToggled()
        {
            _capsLockToggled = _console.ConsoleInput.Input.IsKeyToggled(Keys.CapsLock);
        }
    }
}
