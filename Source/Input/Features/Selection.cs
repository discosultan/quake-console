using System;
using Microsoft.Xna.Framework;
using QuakeConsole.Utilities;

namespace QuakeConsole.Input.Features
{
    internal class Selection
    {
        private ConsoleInput _input;

        private bool _enabled = true;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {                
                _enabled = value;
                if (!_enabled)
                    Clear();
            }
        }

        public void LoadContent(ConsoleInput input)
        {
            _input = input;
            _input.Caret.Moved += (s, e) =>
            {
                if (!_input.ActionDefinitions.AreModifiersAppliedForAction(ConsoleAction.SelectionModifier, _input.Input))
                {
                    Clear();
                    _previousCaretIndex = _input.Caret.Index;
                }
            };
            _input.Cleared += (s, e) => Clear();
            _input.MultiLineInput.LineSwitched += (s, e) =>
            {
                if (_input.ActionDefinitions.AreModifiersAppliedForAction(ConsoleAction.SelectionModifier, _input.Input))
                {
                    if (_selectionActive)
                    {
                        _selectionIndex2 = e.NewLineIndex > e.PreviousLineIndex
                            ? _input.Caret.Index - 1
                            : _input.Caret.Index;
                        _selectionLineIndex2 = _input.MultiLineInput.ActiveLineIndex;
                        CalculateSelectionProperties();
                    }
                    else if (!_selectionActive && (e.CausingAction == ConsoleAction.MoveNextLine ||
                             e.CausingAction == ConsoleAction.MovePreviousLine))
                    {
                        _selectionLineIndex1 = e.PreviousLineIndex;
                        _selectionIndex1 = 0;
                        _selectionIndex2 = e.NewLineIndex > e.PreviousLineIndex
                            ? _input.Caret.Index - 1
                            : _input.Caret.Index;
                        _selectionLineIndex2 = _input.MultiLineInput.ActiveLineIndex;
                        CalculateSelectionProperties();
                    }
                }
            };
        } 

        private int _selectionIndex1;
        private int _selectionIndex2; // Can be -1.
        private int _selectionLineIndex1;
        private int _selectionLineIndex2;

        private int _previousCaretIndex;
        private bool _selectionActive;

        private int _selectionEndIndex;
        private int _selectionLineStartIndex;
        private int _selectionLineEndIndex;

        public bool HasSelection => _selectionActive && SelectionLength > 0;
        public int SelectionStart { get; private set; }
        public int SelectionLength { get; private set; }
        public string SelectionValue => _input.Substring(SelectionStart, SelectionLength); // TODO: fix
        public Color Color { get; set; }

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;

            Caret caret = _input.Caret;
            switch (action)
            {
                case ConsoleAction.MoveLeft:
                case ConsoleAction.MoveRight:
                case ConsoleAction.MoveLeftWord:
                case ConsoleAction.MoveRightWord:
                case ConsoleAction.MoveToBeginning:
                case ConsoleAction.MoveToEnd:
                    if (_input.ActionDefinitions.AreModifiersAppliedForAction(ConsoleAction.SelectionModifier, _input.Input))
                    {
                        if (_selectionActive)
                        {                            
                            _selectionIndex2 = caret.Index;
                            _selectionLineIndex2 = _input.MultiLineInput.ActiveLineIndex;
                            CalculateSelectionProperties();
                        }
                        else
                        {
                            _selectionIndex1 = _previousCaretIndex;
                            _selectionIndex2 = caret.Index;
                            _selectionLineIndex1 = _selectionLineIndex2 = _input.MultiLineInput.ActiveLineIndex;
                            CalculateSelectionProperties();
                            if (SelectionLength > 0)
                                _selectionActive = true;
                        }
                    }
                    else
                    {
                        Clear();
                    }
                    _previousCaretIndex = caret.Index;
                    break;
            }
        }

        public void OnSymbol(Symbol symbol)
        {            
            Clear();                        
            _previousCaretIndex = _input.Caret.Index;
        }

        public void Draw()
        {
            if (!HasSelection) return;
            
            for (int i = _selectionLineStartIndex; i <= _selectionLineEndIndex; i++)
            {
                var line = _input.MultiLineInput.InputLines[i];

                int len;
                int start = i == _selectionLineStartIndex ? SelectionStart : 0;
                if (_selectionLineStartIndex == _selectionLineEndIndex)
                {
                    len = SelectionLength;
                }
                else if (i == _selectionLineStartIndex)
                {
                    len = _input.MultiLineInput.InputLines[i].Length - SelectionStart;
                }
                else if (i == _selectionLineEndIndex)
                {
                    len = _selectionEndIndex + 1;
                }
                else
                {
                    len = _input.MultiLineInput.InputLines[i].Length;
                }

                int startIndex = Math.Max(start - line.VisibleStartIndex, 0);
                int length = Math.Min(len, line.VisibleLength - startIndex);

                var offset = new Vector2(
                    _input.Console.Padding + _input.Console.ConsoleInput.InputPrefixSize.X,
                    _input.Console.WindowArea.Y + _input.Console.WindowArea.Height - _input.Console.Padding - _input.Console.FontSize.Y * (_input.MultiLineInput.InputLines.Count - i));

                float startX = line.MeasureSubstring(0, startIndex).X;
                float width = line.MeasureSubstring(start, length).X;
                var destRectangle = new RectangleF(
                    offset.X + startX,
                    offset.Y,
                    width,
                    _input.Console.FontSize.Y);
                _input.Console.SpriteBatch.Draw(
                    _input.Console.WhiteTexture,
                    destRectangle,
                    Color);
            }
        }

        private void CalculateSelectionProperties()
        {
            if (_selectionLineIndex1 < _selectionLineIndex2)
            {
                SelectionStart = _selectionIndex1;
                _selectionEndIndex = _selectionIndex2;
            }
            else if (_selectionLineIndex2 < _selectionLineIndex1)
            {
                SelectionStart = _selectionIndex2;
                _selectionEndIndex = _selectionIndex1;
            }
            else if (_selectionIndex1 <= _selectionIndex2)
            {
                SelectionStart = _selectionIndex1;
                _selectionEndIndex = _selectionIndex2;
            }
            else
            {
                SelectionStart = _selectionIndex2;
                _selectionEndIndex = _selectionIndex1;
            }

            _selectionLineStartIndex = Math.Min(_selectionLineIndex1, _selectionLineIndex2);            
            _selectionLineEndIndex = Math.Max(_selectionLineIndex1, _selectionLineIndex2);

            if (_selectionLineStartIndex == _selectionLineEndIndex)
                SelectionLength = Math.Abs(_selectionIndex2 - _selectionIndex1);
            else
            {
                SelectionLength = 0;
                for (int i = _selectionLineStartIndex; i <= _selectionLineEndIndex; i++)
                {
                    if (i == _selectionLineStartIndex)
                    {
                        SelectionLength += _input.MultiLineInput.InputLines[i].Length - _selectionLineStartIndex - 1;
                    }
                    else if (i == _selectionLineEndIndex)
                    {
                        SelectionLength += _selectionEndIndex + 1;
                    }
                    else
                    {
                        SelectionLength += _input.MultiLineInput.InputLines[i].Length;
                    }
                }
            }
        }

        private void Clear()
        {
            _selectionActive = false;
            _previousCaretIndex = 0;
            _selectionIndex1 = 0;
            _selectionIndex2 = 0;
            _selectionLineIndex1 = 0;
            _selectionLineIndex2 = 0;
        }
    }
}
