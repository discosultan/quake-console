using System;
using Microsoft.Xna.Framework.Input;

namespace QuakeConsole.Features
{
    internal class CommandExecution
    {
        private Console _console;

        public bool Enabled { get; set; } = true;

        public Action<string> LogInput { get; set; }

        public void LoadContent(Console console) => _console = console;

        public bool ProcessAction(ConsoleAction action)
        {
            if (!Enabled) return false;

            ConsoleInput input = _console.ConsoleInput;
            ConsoleOutput ouput = _console.ConsoleOutput;

            bool hasProcessedAction = false;
            switch (action)
            {
                case ConsoleAction.ExecuteCommand:
                    string cmd = input.Value;
                    // Determine if this is a line break or we should execute command straight away.
                    Keys modifier;
                    if (_console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.NextLineModifier, out modifier) &&
                        input.Input.IsKeyDown(modifier))
                    {
                        ouput.AddCommandEntry(cmd);
                    }
                    else
                    {
                        string executedCmd = cmd;
                        if (ouput.HasCommandEntry)
                            executedCmd = ouput.DequeueCommandEntry() + cmd;

                        // Replace our tab symbols with actual tab characters.
                        executedCmd = executedCmd.Replace(_console.TabSymbol, "\t");
                        // Log the command to be executed if logger is set.
                        LogInput?.Invoke(executedCmd);
                        // Execute command.
                        _console.Interpreter.Execute(ouput, executedCmd);
                    }

                    input.Clear();
                    input.Caret.MoveBy(int.MinValue);
                    hasProcessedAction = true;
                    break;
            }
            return hasProcessedAction;
        }
    }
}
