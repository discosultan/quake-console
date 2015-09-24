using System;
using System.Collections.Generic;
using System.Linq;
using QuakeConsole.Utilities;
using Command = System.Func<string[], string>;

namespace QuakeConsole
{
    /// <summary>
    /// Custom interpreter which executes input commands as user registered types of <see cref="Command"/>.
    /// </summary>
    public class ManualInterpreter : ICommandInterpreter
    {
        private static readonly string[] CommandAndArgumentSeparator = { " " };
        private const StringComparison StringComparisonMethod = StringComparison.OrdinalIgnoreCase;

        // Command map supports executing multiple commands from a single input.
        private readonly Dictionary<string, List<Command>> _commandMap = new Dictionary<string, List<Command>>();
        private string[] _autocompleteEntries;

        /// <summary>
        /// Constructs a new instance of <see cref="ManualInterpreter"/>.
        /// </summary>
        public ManualInterpreter()
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
        /// <param name="output">Console output buffer to append any output messages.</param>
        /// <param name="input">Command to execute.</param>
        public void Execute(IConsoleOutput output, string input)
        {
            if (EchoEnabled) output.Append(input);

            string[] inputSplit = input.Split(CommandAndArgumentSeparator, StringSplitOptions.RemoveEmptyEntries);
            string command = inputSplit[0];
            string[] commandArgs = inputSplit.Skip(1).ToArray();

            List<Command> commandList;
            if (_commandMap.TryGetValue(command, out commandList))
            {
                foreach (Command cmd in commandList)
                {
                    try
                    {
                        string result = cmd(commandArgs);
                        if (!string.IsNullOrWhiteSpace(result))
                            output.Append(result);
                    }
                    catch (Exception ex)
                    {
                        output.Append($"Command '{command}' failed. {ex.Message}");
                    }
                }
            }
            else
            {
                output.Append($"Command '{command}' not found.");
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
        /// Tries to autocomplete the current input value in the <see cref="Console"/> <see cref="ConsoleInput"/>.
        /// </summary>
        /// <param name="input">Console input.</param>
        /// <param name="forward">True if user wants to autocomplete to the next value; false if to the previous value.</param>
        public void Autocomplete(IConsoleInput input, bool forward)
        {
            if (_autocompleteEntries == null)                            
                _autocompleteEntries = _commandMap.Keys.OrderBy(x => x).ToArray();

            string currentInput = input.Value;

            int index = _autocompleteEntries.IndexOf(x => x.Equals(currentInput, StringComparisonMethod));            
            if (index == -1 || input.LastAutocompleteEntry == null) input.LastAutocompleteEntry = currentInput;
            index++;
            if (index >= _autocompleteEntries.Length) index = 0;

            for (int i = index; i < _autocompleteEntries.Length; ++i)
            {
                string commandString = _autocompleteEntries[i];
                if (commandString.StartsWith(input.LastAutocompleteEntry, StringComparisonMethod))
                {
                    input.Clear();
                    input.Write(commandString);                    
                    return;
                }
            }
        }

        /// <summary>
        /// Registers a new command with the interpreter.
        /// </summary>
        /// <param name="commandName">
        /// Name of the command. This is the name user must enter into the <see cref="Console"/> to execute the command.
        /// </param>
        /// <param name="command">Command to interpreter.</param>
        public void RegisterCommand(string commandName, Command command)
        {
            if (commandName == null)
                throw new ArgumentNullException(nameof(commandName));
            if (command == null)
                throw new ArgumentNullException(nameof(command));

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
            foreach (var val in _commandMap.Values)
                val.Remove(command);            
        }
    }
}
