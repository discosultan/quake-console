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
                    string clipboardVal = Clipboard.GetText(TextDataFormat.Text);
                    clipboardVal = clipboardVal.Replace("\t", _input.Console.TabSymbol);
                    _singleElementArray[0] = _input.Console.NewlineSymbol;
                    string[] newlineSplits = clipboardVal.Split(_singleElementArray, StringSplitOptions.None);
                    if (newlineSplits.Length > 1)
                    {
                        //_input.Clear();
                        for (int i = 0; i < newlineSplits.Length - 1; i++)
                            _input.MultiLineInput.AddNewLine(newlineSplits[i]);
                    }
                    _input.Append(newlineSplits[newlineSplits.Length - 1]);
                    break;
            }
        }
    }
}
