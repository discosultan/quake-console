using System;
using System.Windows;
using Microsoft.Xna.Framework.Input;

namespace QuakeConsole.Features
{
    internal class CopyPasting
    {
        private Console _console;
        private readonly string[] _singleElementArray = new string[1];

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
                case ConsoleAction.Cut:
                    _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.CopyPasteModifier, out modifier);
                    if (!input.Input.IsKeyDown(modifier))
                        break;
                    if (input.Selection.HasSelection)
                    {
                        Clipboard.SetText(input.Selection.SelectionValue, TextDataFormat.Text);
                        _console.ConsoleInput.Remove(input.Selection.SelectionStart, input.Selection.SelectionLength);
                    }
                    hasProcessedAction = true;
                    break;
                case ConsoleAction.Copy:
                    _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.CopyPasteModifier, out modifier);
                    if (!input.Input.IsKeyDown(modifier))
                        break;
                    if (input.Selection.HasSelection)
                        Clipboard.SetText(input.Selection.SelectionValue, TextDataFormat.Text);                    
                    hasProcessedAction = true;
                    break;
                case ConsoleAction.Paste:
                    _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.CopyPasteModifier, out modifier);
                    if (!input.Input.IsKeyDown(modifier))
                        break;                    
                    string clipboardVal = Clipboard.GetText(TextDataFormat.Text);
                    clipboardVal = clipboardVal.Replace("\t", _console.TabSymbol);
                    _singleElementArray[0] = _console.NewlineSymbol;
                    string[] newlineSplits = clipboardVal.Split(_singleElementArray, StringSplitOptions.None);
                    if (newlineSplits.Length > 1)
                    {
                        input.Clear();
                        for (int i = 0; i < newlineSplits.Length - 1; i++)
                            _console.ConsoleOutput.AddCommandEntry(newlineSplits[i]);                        
                    }
                    input.Append(newlineSplits[newlineSplits.Length - 1]);
                    hasProcessedAction = true;
                    break;
            }
            return hasProcessedAction;
        }
    }
}
