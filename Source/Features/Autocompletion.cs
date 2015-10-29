namespace QuakeConsole.Features
{
    internal class Autocompletion
    {
        private Console _console;

        public bool Enabled { get; set; } = true;

        public string LastAutocompleteEntry { get; set; }

        public void LoadContent(Console console) => _console = console;

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;
            
            ConsoleInput input = _console.ConsoleInput;

            switch (action)
            {
                case ConsoleAction.AutocompleteForward:
                    _console.Interpreter.Autocomplete(input, true);
                    break;
                case ConsoleAction.AutocompleteBackward:
                    _console.Interpreter.Autocomplete(input, false);
                    break;
                case ConsoleAction.ExecuteCommand:
                    ResetAutocompleteEntry();                    
                    break;
                case ConsoleAction.DeletePreviousChar:                
                    if (input.Length > 0 && input.Caret.Index > 0)
                        ResetAutocompleteEntry();                    
                    break;
                case ConsoleAction.DeleteCurrentChar:
                    if (input.Length > input.Caret.Index)
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
