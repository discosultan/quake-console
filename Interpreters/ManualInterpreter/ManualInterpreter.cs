using System;
using System.Collections.Generic;
using System.Linq;

namespace QuakeConsole
{
    /// <summary>
    /// Interpreter which executes input as user defined commands.
    /// </summary>
    public class ManualInterpreter : ICommandInterpreter
    {
        private static readonly string[] CommandAndArgumentSeparator = { " " };
        private static readonly string[] InstructionSeparator = { ";" };
        private const StringComparison StringComparisonMethod = StringComparison.OrdinalIgnoreCase;

        // Command map supports executing multiple commands from a single input.
        private readonly Dictionary<string, List<Func<string[], string>>> _commandMap = new Dictionary<string, List<Func<string[], string>>>();
        private string[] _autocompleteEntries;        

        /// <summary>
        /// Gets or sets if the user entered command should be shown in the output.
        /// </summary>
        public bool EchoEnabled { get; set; } = true;

        /// <summary>
        /// Executes a command that is matched by the first input word.
        /// </summary>
        /// <param name="output">Console output to append any output messages.</param>
        /// <param name="input">Command to execute.</param>
        public void Execute(IConsoleOutput output, string input)
        {
            if (EchoEnabled) output.Append(input);

            string[] instructions = input.Split(InstructionSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var instruction in instructions)
            {
                string[] inputSplit = instruction.Trim().Split(CommandAndArgumentSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (inputSplit.Length == 0) return;

                string command = inputSplit[0];
                string[] commandArgs = inputSplit.Skip(1).ToArray();

                List<Func<string[], string>> commandList;
                if (_commandMap.TryGetValue(command, out commandList))
                {
                    foreach (Func<string[], string> cmd in commandList)
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

            if (_autocompleteEntries.Length == 0)
                return;

            string currentInput = input.Value;

            int index = _autocompleteEntries.IndexOf(x => x.Equals(currentInput, StringComparisonMethod));            
            if (index == -1 || input.LastAutocompleteEntry == null)
                input.LastAutocompleteEntry = currentInput;

            if (forward)
            {
                index = (index + 1)%_autocompleteEntries.Length;
                for (int i = index; i < _autocompleteEntries.Length; ++i)
                    if (TryAutocomplete(input, _autocompleteEntries[i]))
                        return;
            }
            else
            {
                index--;
                if (index == -1)
                    index = _autocompleteEntries.Length - 1;
                for (int i = index; i >= 0; --i)
                    if (TryAutocomplete(input, _autocompleteEntries[i]))
                        return;
            }
        }

        /// <summary>
        /// Registers a new command with the interpreter.
        /// </summary>
        /// <param name="commandName">
        /// Name of the command. This is the name user must enter into the <see cref="Console"/> to execute the command.
        /// </param>
        /// <param name="command">Command to interpreter.</param>
        public void RegisterCommand(string commandName, Action<string[]> command)
        {
            Func<string[], string> commandWithReturnVal = args =>
            {
                command(args);
                return "";
            };
            RegisterCommand(commandName, commandWithReturnVal);
        }

        /// <summary>
        /// Registers a new command with the interpreter.
        /// </summary>
        /// <param name="commandName">
        /// Name of the command. This is the name user must enter into the <see cref="Console"/> to execute the command.
        /// </param>
        /// <param name="command">Command to interpreter.</param>
        public void RegisterCommand(string commandName, Func<string[], string> command)
        {
            if (commandName == null)
                throw new ArgumentNullException(nameof(commandName));
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            List<Func<string[], string>> commandList;
            if (!_commandMap.TryGetValue(commandName, out commandList))
            {
                commandList = new List<Func<string[], string>>();
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

        private static bool TryAutocomplete(IConsoleInput input, string commandString)
        {
            if (commandString.StartsWith(input.LastAutocompleteEntry, StringComparisonMethod))
            {
                input.Clear();
                input.Append(commandString);
                return true;
            }
            return false;
        }
    }
}
