using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace QuakeConsole.Input.Features
{
    internal class InputHistory
    {
        // Input history.        
        private readonly List<string> _inputHistory = new List<string>();
        private int _inputHistoryIndexer;
        private bool _inputHistoryDoNotDecrement;

        private ConsoleInput _input;

        public bool Enabled { get; set; } = true;

        public void LoadContent(ConsoleInput input) => _input = input;

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;

            string cmd = _input.Value;

            switch (action)
            {
                case ConsoleAction.ExecuteCommand:
                case ConsoleAction.NewLine:
                    // If the cmd matches the currently indexed historical entry then set a special flag
                    // which when moving backward in history, does not actually move backward, but will instead
                    // return the same entry that was returned before. This is similar to how Powershell and Cmd Prompt work.

                    if (_inputHistory.Count == 0 || _inputHistoryIndexer == int.MaxValue || !_inputHistory[_inputHistoryIndexer].Equals(cmd))
                        _inputHistoryIndexer = int.MaxValue;
                    else
                        _inputHistoryDoNotDecrement = true;

                    // Find the last historical entry if any.
                    string lastHistoricalEntry = null;
                    if (_inputHistory.Count > 0)
                        lastHistoricalEntry = _inputHistory[_inputHistory.Count - 1];

                    // Only add current command to input history if it is not an empty string and
                    // does not match the last historical entry.
                    if (cmd != "" && !cmd.Equals(lastHistoricalEntry, StringComparison.Ordinal))
                        _inputHistory.Add(cmd);
                    break;
                case ConsoleAction.PreviousCommandInHistory:
                    if (!_inputHistoryDoNotDecrement)
                        _inputHistoryIndexer--;
                    ManageHistory();
                    break;
                case ConsoleAction.NextCommandInHistory:
                    _inputHistoryIndexer++;
                    ManageHistory();
                    break;
                case ConsoleAction.AutocompleteForward:
                    _inputHistoryIndexer = int.MaxValue;                    
                    break;
            }
        }

        public void OnSymbol(Symbol symbol)
        {
            _inputHistoryIndexer = int.MaxValue;
        }

        public void Clear()
        {
            _inputHistory.Clear();
            _inputHistoryIndexer = int.MaxValue;
            _inputHistoryDoNotDecrement = false;
        }

        private void ManageHistory()
        {
            // Check if there are any entries in the history.
            if (_inputHistory.Count <= 0) return;

            _inputHistoryIndexer = MathHelper.Clamp(_inputHistoryIndexer, 0, _inputHistory.Count - 1);

            _inputHistoryDoNotDecrement = false;
            _input.LastAutocompleteEntry = null;
            _input.Value = _inputHistory[_inputHistoryIndexer];
        }
    }
}
