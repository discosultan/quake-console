using System;

namespace QuakeConsole.Features
{
    internal class Deletion
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
                case ConsoleAction.DeletePreviousChar:
                    if (input.Selection.HasSelection)
                        input.Remove(input.Selection.SelectionStart, input.Selection.SelectionLength);   
                    else if (input.Length > 0 && input.Caret.Index > 0)
                        input.Remove(Math.Max(0, input.Caret.Index - 1), 1);
                    break;
                case ConsoleAction.DeleteCurrentChar:
                    if (input.Selection.HasSelection)
                        input.Remove(input.Selection.SelectionStart, input.Selection.SelectionLength);
                    else if (input.Length > input.Caret.Index)
                        input.Remove(input.Caret.Index, 1);
                    break;
            }
        }
    }
}
