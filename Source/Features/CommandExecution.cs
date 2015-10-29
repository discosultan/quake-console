using System;

namespace QuakeConsole.Features
{
    internal class CommandExecution
    {
        private Console _console;

        public bool Enabled { get; set; } = true;

        public Action<string> LogInput { get; set; }

        public void LoadContent(Console console) => _console = console;

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;

            ConsoleInput input = _console.ConsoleInput;
            ConsoleOutput ouput = _console.ConsoleOutput;

            switch (action)
            {
                case ConsoleAction.ExecuteCommand:
                    string cmd = input.Value;
                    string executedCmd = cmd;
                    if (ouput.HasCommandEntry)
                        executedCmd = ouput.DequeueCommandEntry() + cmd;

                    // Replace our tab symbols with actual tab characters.
                    executedCmd = executedCmd.Replace(_console.TabSymbol, "\t");
                    // Log the command to be executed if logger is set.
                    LogInput?.Invoke(executedCmd);
                    // Execute command.
                    _console.Interpreter.Execute(ouput, executedCmd);
                    input.Clear();
                    input.Caret.MoveBy(int.MinValue);
                    break;
            }
        }
    }
}
