using System;

namespace QuakeConsole.Input.Features
{
    internal class Deletion
    {
        private ConsoleInput _input;

        public bool Enabled { get; set; } = true;

        public void LoadContent(ConsoleInput input) => _input = input;

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;

            switch (action)
            {
                case ConsoleAction.DeletePreviousChar:
                    if (_input.Selection.HasSelection)
                        _input.Remove(_input.Selection.SelectionStart, _input.Selection.SelectionLength);                       
                    else if (_input.Length > 0 && _input.CaretIndex > 0)
                        _input.Remove(Math.Max(0, _input.CaretIndex - 1), 1);
                    break;
                case ConsoleAction.DeleteCurrentChar:
                    if (_input.Selection.HasSelection)
                        _input.Remove(_input.Selection.SelectionStart, _input.Selection.SelectionLength);                    
                    else if (_input.Length > _input.CaretIndex)
                        _input.Remove(_input.CaretIndex, 1);
                    break;
            }
        }
    }
}
