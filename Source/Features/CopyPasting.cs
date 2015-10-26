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

            Keys modifier;
            bool hasProcessedAction = false;                        
            switch (action)
            {
                case ConsoleAction.Copy:
                    _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.CopyPasteModifier, out modifier);
                    if (!_console.Input.IsKeyDown(modifier))
                        break;
                    hasProcessedAction = true;
                    break;
                case ConsoleAction.Paste:
                    _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.CopyPasteModifier, out modifier);
                    if (!_console.Input.IsKeyDown(modifier))
                        break;
                    // TODO: Enable clipboard pasting. How to approach this in a cross-platform manner?
                    //string clipboardVal = Clipboard.GetText(TextDataFormat.Text);
                    //_currentInput.Append(clipboardVal);
                    //MoveCaret(clipboardVal.Length);
                    hasProcessedAction = true;
                    break;
            }
            return hasProcessedAction;
        }
    }
}
