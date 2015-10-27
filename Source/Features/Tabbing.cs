using System;
using Microsoft.Xna.Framework.Input;

namespace QuakeConsole.Features
{
    internal class Tabbing
    {
        private Console _console;        

        public bool Enabled { get; set; } = true;        

        public void LoadContent(Console console) => _console = console;

        public bool ProcessAction(ConsoleAction action)
        {
            if (!Enabled) return false;            

            ConsoleInput input = _console.ConsoleInput;

            bool hasProcessedAction = false;
            switch (action)
            {
                case ConsoleAction.Tab:
                    Keys modifier;
                    _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.TabModifier, out modifier);
                    if (input.Input.IsKeyDown(modifier))
                        RemoveTab();
                    else
                        input.Append(_console.TabSymbol);
                    hasProcessedAction = true;
                    break;
            }
            return hasProcessedAction;
        }

        public void RemoveTab()
        {
            ConsoleInput input = _console.ConsoleInput;

            bool isTab = true;
            int counter = 0;
            for (int i = input.Caret.Index - 1; i >= 0; i--)
            {
                if (counter >= _console.TabSymbol.Length) break;
                if (input[i] != _console.TabSymbol[_console.TabSymbol.Length - counter++ - 1])
                {
                    isTab = false;
                    break;
                }
            }
            int numToRemove = counter;
            if (isTab)
                input.Remove(Math.Max(0, input.Caret.Index - _console.TabSymbol.Length), numToRemove);                
            input.Caret.MoveBy(-_console.TabSymbol.Length);
        }
    }
}
