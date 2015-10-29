using System;

namespace QuakeConsole.Features
{
    internal class Tabbing
    {
        private Console _console;        

        public bool Enabled { get; set; } = true;        

        public void LoadContent(Console console) => _console = console;

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;            

            ConsoleInput input = _console.ConsoleInput;

            switch (action)
            {
                case ConsoleAction.Tab:
                    input.Append(_console.TabSymbol);
                    break;
                case ConsoleAction.RemoveTab:
                    RemoveTab();
                    break;
            }
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
