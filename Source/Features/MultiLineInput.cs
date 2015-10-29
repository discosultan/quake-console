namespace QuakeConsole.Features
{
    internal class MultiLineInput
    {
        private Console _console;

        public bool Enabled { get; set; } = true;

        public void LoadContent(Console console) => _console = console;

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;

            ConsoleInput input = _console.ConsoleInput;
            ConsoleOutput ouput = _console.ConsoleOutput;

            switch (action)
            {
                case ConsoleAction.NewLine:
                    ouput.AddCommandEntry(input.Value);
                    input.Clear();
                    input.Caret.MoveBy(int.MinValue);
                    break;
            }
        }
    }
}
