using System;
using System.Windows;

namespace QuakeConsole.Input.Features
{
    internal class CopyPasting
    {
        private ConsoleInput _input;
        private readonly string[] _singleElementArray = new string[1];

        public bool Enabled { get; set; } = true;

        public void LoadContent(ConsoleInput input) => _input = input;

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;

            switch (action)
            {
                case ConsoleAction.Cut:
                    if (_input.Selection.HasSelection)
                    {
                        Clipboard.SetText(_input.Selection.SelectionValue, TextDataFormat.Text);
                        _input.Remove(_input.Selection.SelectionStart, _input.Selection.SelectionLength);
                    }
                    break;
                case ConsoleAction.Copy:
                    if (_input.Selection.HasSelection)
                        Clipboard.SetText(_input.Selection.SelectionValue, TextDataFormat.Text);                    
                    break;
                case ConsoleAction.Paste:
                    string clipboardVal = Clipboard.GetText(TextDataFormat.Text).Replace("\n", _input.Console.NewlineSymbol);
                    clipboardVal = clipboardVal.Replace("\t", _input.Console.TabSymbol);
                    _singleElementArray[0] = _input.Console.NewlineSymbol;
                    string[] newlineSplits = clipboardVal.Split(_singleElementArray, StringSplitOptions.None);
                    if (newlineSplits.Length > 1)
                    {                                                
                        for (int i = 0; i < newlineSplits.Length - 1; i++)
                        {
                            string entry = newlineSplits[i];
                            if (i == 0)
                                entry = _input.Substring(0, _input.Caret.Index) + entry;

                            _input.Console.ConsoleOutput.AddCommandEntry(entry);
                        }
                        _input.Remove(0, _input.Caret.Index);
                    }
                    _input.Append(newlineSplits[newlineSplits.Length - 1]);
                    break;
            }
        }
    }
}
