using System;
using System.Collections.Generic;
using System.Linq;
using Varus.Paradox.Console.CustomInterpreter.Utilities;

namespace Varus.Paradox.Console.CustomInterpreter
{
    public class CustomCommandInterpreter : ICommandInterpreter
    {
        private static readonly string[] CommandAndArgumentSeparator = { " " };
        private const StringComparison StringComparisonMethod = StringComparison.OrdinalIgnoreCase;

        // Command map supports executing multiple commands from a single input.
        private readonly Dictionary<string, List<Command>> _commandMap = new Dictionary<string, List<Command>>();
        private string[] _autocompleteEntries;

        public CustomCommandInterpreter()
        {
            EchoEnabled = true;
        }

        public bool EchoEnabled { get; set; }

        public void Execute(OutputBuffer outputBuffer, string input)
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

        public void Autocomplete(InputBuffer inputBuffer, bool isNextValue)
        {
            if (_autocompleteEntries == null)                            
                _autocompleteEntries = _commandMap.Keys.OrderBy(x => x).ToArray();

            string currentInput = inputBuffer.Get();

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

        public void UnregisterCommand(string commandName)
        {
            _commandMap.Remove(commandName);
        }

        public void UnregisterCommand(Command command)
        {
            _commandMap.Values.ForEach(x => x.Remove(command));
        }
    }
}
