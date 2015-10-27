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
                    if (input.Input.IsKeyDown(modifier))
                        MoveToPreviousWord();
                    else
                        input.Caret.MoveBy(-1);
                    hasProcessedAction = true;
                    break;
                case ConsoleAction.MoveRight:
                    _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.MoveByWordModifier, out modifier);
                    if (input.Input.IsKeyDown(modifier))
                        MoveToNextWord();
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

        public void MoveToPreviousWord()
        {
            ConsoleInput input = _console.ConsoleInput;
            Caret caret = input.Caret;
            bool prevOnLetter = caret.Index < input.Length && char.IsLetterOrDigit(input[caret.Index]);
            for (int i = caret.Index - 1; i >= 0; i--)
            {
                bool currentOnLetter = char.IsLetterOrDigit(input[i]);
                if (prevOnLetter && !currentOnLetter && i != caret.Index - 1)
                {
                    caret.Index = i + 1;
                    return;
                }
                prevOnLetter = currentOnLetter;
            }
            caret.Index = 0;
        }

        public void MoveToNextWord()
        {
            ConsoleInput input = _console.ConsoleInput;
            Caret caret = input.Caret;
            bool prevOnLetter = caret.Index < input.Length && char.IsLetterOrDigit(input[caret.Index]);
            for (int i = caret.Index + 1; i < input.Length; i++)
            {
                bool currentOnLetter = char.IsLetterOrDigit(input[i]);
                if (!prevOnLetter && currentOnLetter)
                {
                    caret.Index = i;
                    return;
                }
                prevOnLetter = currentOnLetter;
            }
            caret.Index = input.Length;
        }
    }
}
