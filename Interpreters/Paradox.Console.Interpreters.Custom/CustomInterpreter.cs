using System;
using System.Collections.Generic;
using System.Linq;
using Varus.Paradox.Console.Interpreters.Custom.Utilities;

namespace Varus.Paradox.Console.Interpreters.Custom
{
    /// <summary>
    /// Custom interpreter which executes input commands as user registered types of <see cref="Command"/>.
    /// </summary>
    public class CustomInterpreter : ICommandInterpreter
    {
        private static readonly string[] CommandAndArgumentSeparator = { " " };
        private const StringComparison StringComparisonMethod = StringComparison.OrdinalIgnoreCase;

        // Command map supports executing multiple commands from a single input.
        private readonly Dictionary<string, List<Command>> _commandMap = new Dictionary<string, List<Command>>();
        private string[] _autocompleteEntries;

        /// <summary>
        /// Constructs a new instance of <see cref="CustomInterpreter"/>.
        /// </summary>
        public CustomInterpreter()
        {
            EchoEnabled = true;
        }

        /// <summary>
        /// Gets or sets if the user entered command should be shown in the output.
        /// </summary>
        public bool EchoEnabled { get; set; }

        /// <summary>
        /// Executes a command by looking if any <see cref="Command"/> is registered with that name and
        /// runs it if it is.
        /// </summary>
        /// <param name="outputBuffer">Console output buffer to append any output messages.</param>
        /// <param name="input">Command to execute.</param>
        public void Execute(IOutputBuffer outputBuffer, string input)
        {
            if (EchoEnabled) outputBuffer.Append(input);

            string[] inputSplit = input.Split(CommandAndArgumentSeparator, StringSplitOptions.RemoveEmptyEntries);
            string command = inputSplit[0];
            string[] commandArgs = inputSplit.Skip(1).ToArray();

            List<Command> commandList;
            if (_commandMap.TryGetValue(command, out commandList))
            {
                foreach (Command cmd in commandList)
                {
                    CommandResult result = cmd.Execute(commandArgs);
                    if (result.IsFaulted)
                    {
                        outputBuffer.Append(string.Format("Command '{0}' failed. {1}", command, result.Message ?? ""));
                    }
                    else if (!string.IsNullOrWhiteSpace(result.Message))
                    {
                        outputBuffer.Append(result.Message);
                    }
                }
            }
            else
            {
                outputBuffer.Append(string.Format("Command '{0}' not found.", command));
            }
        }

        /// <summary>
        /// Resets the interpreter by clearing any registered commands.
        /// </summary>
        public void Reset()
        {
            _autocompleteEntries = null;
            _commandMap.Clear();
        }

        /// <summary>
        /// Tries to autocomplete the current input value in the <see cref="ConsoleShell"/> <see cref="InputBuffer"/>.
        /// </summary>
        /// <param name="inputBuffer">Console input.</param>
        /// <param name="forward">True if user wants to autocomplete to the next value; false if to the previous value.</param>
        public void Autocomplete(IInputBuffer inputBuffer, bool forward)
        {
            if (_autocompleteEntries == null)                            
                _autocompleteEntries = _commandMap.Keys.OrderBy(x => x).ToArray();

            string currentInput = inputBuffer.Value;

            int index = _autocompleteEntries.IndexOf(x => x.Equals(currentInput, StringComparisonMethod));
            if (index == -1 || inputBuffer.LastAutocompleteEntry == null) inputBuffer.LastAutocompleteEntry = currentInput;
            index++;
            if (index >= _autocompleteEntries.Length) index = 0;

            for (int i = index; i < _autocompleteEntries.Length; ++i)
            {
                string commandString = _autocompleteEntries[i];
                if (commandString.StartsWith(inputBuffer.LastAutocompleteEntry, StringComparisonMethod))
                {
                    inputBuffer.Clear();
                    inputBuffer.Write(commandString);                    
                    return;
                }
            }
        }

        /// <summary>
        /// Registers a new command with the interpreter.
        /// </summary>
        /// <param name="commandName">
        /// Name of the command. This is the name user must enter into the <see cref="ConsoleShell"/> to execute the command.
        /// </param>
        /// <param name="command">Command to interpreter.</param>
        public void RegisterCommand(string commandName, Command command)
        {
            Check.ArgumentNotNull(commandName, "commandName");
            Check.ArgumentNotNull(command, "command");

            List<Command> commandList;
            if (!_commandMap.TryGetValue(commandName, out commandList))
            {
                commandList = new List<Command>();
                _commandMap.Add(commandName, commandList);
            }
            commandList.Add(command);

            // We set autocomplete related command values to null. They will be reinitialized
            // next time accessed.
            _autocompleteEntries = null;
        }

        /// <summary>
        /// Unregisters a command with the provided name if any.
        /// </summary>
        /// <param name="commandName">Command name to remove.</param>
        public void UnregisterCommand(string commandName)
        {
            _commandMap.Remove(commandName);
        }

        /// <summary>
        /// Unregisters the provided command if found.
        /// </summary>
        /// <param name="command">Command to remove.</param>
        public void UnregisterCommand(Command command)
        {
            _commandMap.Values.ForEach(x => x.Remove(command));
        }
    }
}
