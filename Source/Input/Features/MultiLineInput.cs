namespace QuakeConsole.Input.Features
{
    internal class MultiLineInput
    {
        private ConsoleInput _input;

        public bool Enabled { get; set; } = true;

        public void LoadContent(ConsoleInput input) => _input = input;

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;

            switch (action)
            {
                case ConsoleAction.NewLine:
                    _input.Console.ConsoleOutput.AddCommandEntry(_input.Value);
                    _input.Clear();
                    _input.Caret.MoveBy(int.MinValue);
                    break;
            }
        }
    }
}
