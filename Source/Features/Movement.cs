using Microsoft.Xna.Framework.Input;

namespace QuakeConsole.Features
{
    internal class Movement
    {
        private Console _console;

        public bool Enabled { get; set; } = true;

        public void LoadContent(Console console) => _console = console;

        public bool ProcessAction(ConsoleAction action)
        {
            if (!Enabled) return false;

            ConsoleInput input = _console.ConsoleInput;

            Keys modifier;
            bool hasProcessedAction = false;
            switch (action)
            {
                case ConsoleAction.MoveLeft:
                    _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.MoveByWordModifier, out modifier);
                    if (_console.Input.IsKeyDown(modifier))
                        input.Caret.MoveToPreviousWord();
                    else
                        input.Caret.MoveBy(-1);
                    hasProcessedAction = true;
                    break;
                case ConsoleAction.MoveRight:
                    _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.MoveByWordModifier, out modifier);
                    if (_console.Input.IsKeyDown(modifier))
                        input.Caret.MoveToNextWord();
                    else
                        input.Caret.MoveBy(1);
                    hasProcessedAction = true;
                    break;
                case ConsoleAction.MoveToBeginning:
                    input.Caret.Index = 0;
                    hasProcessedAction = true;
                    break;
                case ConsoleAction.MoveToEnd:
                    input.Caret.Index = input.Length;
                    hasProcessedAction = true;
                    break;
            }
            return hasProcessedAction;
        }
    }
}
