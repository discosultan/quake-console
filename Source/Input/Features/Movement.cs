namespace QuakeConsole.Input.Features
{
    internal class Movement
    {
        private ConsoleInput _input;

        public bool Enabled { get; set; } = true;

        public void LoadContent(ConsoleInput input) => _input = input;

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;

            switch (action)
            {
                case ConsoleAction.MoveLeft:
                    _input.Caret.MoveBy(-1);
                    break;
                case ConsoleAction.MoveLeftWord:
                    MoveToPreviousWord();
                    break;
                case ConsoleAction.MoveRight:
                    _input.Caret.MoveBy(1);
                    break;
                case ConsoleAction.MoveRightWord:
                    MoveToNextWord();
                    break;
                case ConsoleAction.MoveToBeginning:
                    _input.Caret.Index = 0;
                    break;
                case ConsoleAction.MoveToEnd:
                    _input.Caret.Index = _input.Length;
                    break;
            }
        }

        public void MoveToPreviousWord()
        {
            Caret caret = _input.Caret;
            bool prevOnLetter = caret.Index < _input.Length && char.IsLetterOrDigit(_input[caret.Index]);
            for (int i = caret.Index - 1; i >= 0; i--)
            {
                bool currentOnLetter = char.IsLetterOrDigit(_input[i]);
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
            Caret caret = _input.Caret;
            bool prevOnLetter = caret.Index < _input.Length && char.IsLetterOrDigit(_input[caret.Index]);
            for (int i = caret.Index + 1; i < _input.Length; i++)
            {
                bool currentOnLetter = char.IsLetterOrDigit(_input[i]);
                if (!prevOnLetter && currentOnLetter)
                {
                    caret.Index = i;
                    return;
                }
                prevOnLetter = currentOnLetter;
            }
            caret.Index = _input.Length;
        }
    }
}
