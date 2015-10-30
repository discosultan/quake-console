namespace QuakeConsole.Input.Features
{
    internal class Autocompletion
    {
        private ConsoleInput _input;

        public bool Enabled { get; set; } = true;

        public string LastAutocompleteEntry { get; set; }

        public void LoadContent(ConsoleInput console) => _input = console;

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;
            
            switch (action)
            {
                case ConsoleAction.AutocompleteForward:
                    _input.Console.Interpreter.Autocomplete(_input, true);
                    break;
                case ConsoleAction.AutocompleteBackward:
                    _input.Console.Interpreter.Autocomplete(_input, false);
                    break;
                case ConsoleAction.ExecuteCommand:
                    ResetAutocompleteEntry();                    
                    break;
                case ConsoleAction.DeletePreviousChar:                
                    if (_input.Length > 0 && _input.Caret.Index > 0)
                        ResetAutocompleteEntry();                    
                    break;
                case ConsoleAction.DeleteCurrentChar:
                    if (_input.Length > _input.Caret.Index)
                        ResetAutocompleteEntry();
                    break;
                case ConsoleAction.Paste:
                case ConsoleAction.Cut:
                case ConsoleAction.Tab:
                    ResetAutocompleteEntry();
                    break;                                    
            }            
        }

        public void OnSymbol(Symbol symbol)
        {
            ResetAutocompleteEntry();
        }

        private void ResetAutocompleteEntry()
        {
            LastAutocompleteEntry = null;
        }
    }
}
