namespace QuakeConsole.Features
{
    internal class Movement
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
                case ConsoleAction.MoveLeft:
                    input.Caret.MoveBy(-1);
                    break;
                case ConsoleAction.MoveLeftWord:
                    MoveToPreviousWord();
                    break;
                case ConsoleAction.MoveRight:
                    input.Caret.MoveBy(1);
                    break;
                case ConsoleAction.MoveRightWord:
                    MoveToNextWord();
                    break;
                case ConsoleAction.MoveToBeginning:
                    input.Caret.Index = 0;
                    break;
                case ConsoleAction.MoveToEnd:
                    input.Caret.Index = input.Length;
                    break;
            }
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
