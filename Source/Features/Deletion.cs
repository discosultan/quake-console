using System;

namespace QuakeConsole.Features
{
    internal class Deletion
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
                case ConsoleAction.DeletePreviousChar:
                    if (input.Length > 0 && input.Caret.Index > 0)
                        input.Remove(Math.Max(0, input.Caret.Index - 1), 1);
                    hasProcessedAction = true;
                    break;
                case ConsoleAction.DeleteCurrentChar:
                    if (input.Length > input.Caret.Index)
                        input.Remove(input.Caret.Index, 1);
                    hasProcessedAction = true;
                    break;
            }
            return hasProcessedAction;
        }
    }
}
