using Microsoft.Xna.Framework.Input;

namespace QuakeConsole
{
    internal class CaseSensitivity
    {
        private ConsoleInput _input;
        private bool _capsLockToggled;

        public void LoadContent(ConsoleInput input)
        {
            _input = input;
            CheckKeysToggled();
        }        

        public void OnAction(ConsoleAction action)
        {
            switch (action)
            {
                case ConsoleAction.CapsLock:
                    CheckKeysToggled();
                    break;
            }
        }

        public string ProcessSymbol(Symbol symbol)
        {
            bool capsLockApplies = symbol.Lowercase.Length == 1 && char.IsLetter(symbol.Lowercase[0]) && _capsLockToggled;
            bool uppercaseModifierApplies = _input.ActionDefinitions.AreModifiersAppliedForAction(
                ConsoleAction.UppercaseModifier, _input.Input);

            return capsLockApplies ^ uppercaseModifierApplies 
                ? symbol.Uppercase 
                : symbol.Lowercase;
        }

        private void CheckKeysToggled()
        {
            _capsLockToggled = _input.Input.IsKeyToggled(Keys.CapsLock);
        }
    }
}
