using System;
using QuakeConsole.Output;

namespace QuakeConsole.Input.Features
{
    internal class CommandExecution
    {
        private ConsoleInput _input;

        public bool Enabled { get; set; } = true;

        public Action<string> LogInput { get; set; }

        public void LoadContent(ConsoleInput input) => _input = input;

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;
            
            ConsoleOutput output = _input.Console.ConsoleOutput;

            switch (action)
            {
                case ConsoleAction.ExecuteCommand:
                    string cmd = _input.Value;
                    // Replace our tab symbols with actual tab characters.
                    cmd = cmd.Replace(_input.Console.TabSymbol, "\t");
                    // Log the command to be executed if logger is set.
                    LogInput?.Invoke(cmd);
                    // Execute command.
                    _input.Console.Interpreter.Execute(output, cmd);
                    ResetInput();
                    break;
                case ConsoleAction.NewLine:
                    output.AddCommandEntry(_input.Value);
                    ResetInput();
                    break;
            }
        }

        private void ResetInput()
        {
            _input.Clear();
            _input.Caret.MoveBy(int.MinValue);
        }
    }
}
