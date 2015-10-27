using System.Windows;
using Microsoft.Xna.Framework.Input;

namespace QuakeConsole.Features
{
    internal class CopyPasting
    {
        private Console _console;

        public bool Enabled { get; set; } = true;

        public void LoadContent(Console console) => _console = console;

        public bool ProcessAction(ConsoleAction action)
        {
            if (!Enabled) return false;

            ConsoleInput input = _console.ConsoleInput;

            Keys modifier;
            bool hasProcessedAction = false;                        
            switch (action)
            {
                case ConsoleAction.Copy:
                    _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.CopyPasteModifier, out modifier);
                    if (!input.Input.IsKeyDown(modifier))
                        break;
                    // TODO: implement
                    hasProcessedAction = true;
                    break;
                case ConsoleAction.Paste:
                    _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.CopyPasteModifier, out modifier);
                    if (!input.Input.IsKeyDown(modifier))
                        break;                    
                    string clipboardVal = Clipboard.GetText(TextDataFormat.Text);
                    input.Append(clipboardVal);                    
                    hasProcessedAction = true;
                    break;
            }
            return hasProcessedAction;
        }
    }
}
