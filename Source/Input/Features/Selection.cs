using System;
using Microsoft.Xna.Framework;
using QuakeConsole.Utilities;

namespace QuakeConsole.Input.Features
{
    internal class Selection
    {
        private ConsoleInput _input;

        private int _selectionIndex1;
        private int _selectionIndex2; // Can be -1.

        private int _previousCaretIndex;
        private bool _selectionActive;

        private int _selectionEndIndex;        

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

        public bool HasSelection => _selectionActive && SelectionLength > 0;
        public int SelectionStart { get; private set; }
        public int SelectionLength { get; private set; }
        public string SelectionValue => _input.Substring(SelectionStart, SelectionLength); // TODO: fix
        public Color Color { get; set; }

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
        }

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
                            CalculateSelectionProperties();
                        }
                        else
                        {
                            _selectionIndex1 = _previousCaretIndex;
                            _selectionIndex2 = caret.Index;
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
            
            int startIndex = Math.Max(SelectionStart - _input.VisibleStartIndex, 0);
            int length = Math.Min(SelectionLength, _input.VisibleLength - startIndex);

            var offset = new Vector2(
                _input.Console.Padding + _input.Console.ConsoleInput.InputPrefixSize.X,
                _input.Console.WindowArea.Y + _input.Console.WindowArea.Height - _input.Console.Padding - _input.Console.FontSize.Y);

            float startX = _input.MeasureSubstring(0, startIndex).X;
            float width = _input.MeasureSubstring(SelectionStart, length).X;
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

        private void CalculateSelectionProperties()
        {
            if (_selectionIndex1 <= _selectionIndex2)
            {
                SelectionStart = _selectionIndex1;
                _selectionEndIndex = _selectionIndex2;
            }
            else
            {
                SelectionStart = _selectionIndex2;
                _selectionEndIndex = _selectionIndex1;
            }
            SelectionLength = Math.Abs(_selectionIndex2 - _selectionIndex1);
        }

        private void Clear()
        {
            _selectionActive = false;
            _previousCaretIndex = 0;
            _selectionIndex1 = 0;
            _selectionIndex2 = 0;            
        }
    }
}
