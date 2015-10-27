using Microsoft.Xna.Framework.Input;

namespace QuakeConsole.Features
{
    internal class Autocompletion
    {
        private Console _console;

        public bool Enabled { get; set; } = true;

        public string LastAutocompleteEntry { get; set; }

        public void LoadContent(Console console) => _console = console;

        public bool ProcessAction(ConsoleAction action)
        {
            if (!Enabled) return false;
            
            ConsoleInput input = _console.ConsoleInput;

            bool hasProcessedAction = false;
            switch (action)
            {
                case ConsoleAction.Autocomplete:
                    Keys modifier;
                    bool hasModifier = _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.AutocompleteModifier, out modifier);
                    if (!hasModifier || input.Input.IsKeyDown(modifier))
                    {                       
                        bool canMoveBackwards = _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.PreviousEntryModifier, out modifier);
                        _console.Interpreter.Autocomplete(input, !canMoveBackwards || !input.Input.IsKeyDown(modifier));
                        hasProcessedAction = true;
                    }
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
                    // TODO: Reset on paste
                    //case ConsoleAction.Paste:
                    //    _actionDefinitions.BackwardTryGetValue(ConsoleAction.CopyPasteModifier, out modifier);
                    //    if (!Input.IsKeyDown(modifier))
                    //        break;                                        
                    //break;
                case ConsoleAction.Tab:
                    ResetAutocompleteEntry();                    
                    break;
            }
            return hasProcessedAction;
        }

        public void ProcessSymbol(Symbol symbol)
        {
            ResetAutocompleteEntry();
        }

        private void ResetAutocompleteEntry()
        {
            LastAutocompleteEntry = null;
        }
    }
}
