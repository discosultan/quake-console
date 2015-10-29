using System;
using System.Windows;

namespace QuakeConsole.Features
{
    internal class CopyPasting
    {
        private Console _console;
        private readonly string[] _singleElementArray = new string[1];

        public bool Enabled { get; set; } = true;

        public void LoadContent(Console console) => _console = console;

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;

            ConsoleInput input = _console.ConsoleInput;

            switch (action)
            {
                case ConsoleAction.Cut:
                    if (input.Selection.HasSelection)
                    {
                        Clipboard.SetText(input.Selection.SelectionValue, TextDataFormat.Text);
                        _console.ConsoleInput.Remove(input.Selection.SelectionStart, input.Selection.SelectionLength);
                    }
                    break;
                case ConsoleAction.Copy:
                    if (input.Selection.HasSelection)
                        Clipboard.SetText(input.Selection.SelectionValue, TextDataFormat.Text);                    
                    break;
                case ConsoleAction.Paste:
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
                    break;
            }
        }
    }
}
