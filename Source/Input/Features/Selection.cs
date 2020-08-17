using System;
using Microsoft.Xna.Framework;

namespace QuakeConsole
{
    internal class Selection
    {
        private ConsoleInput _input;

        private int _selectionIndex1;
        private int _selectionIndex2;

        private int _previousCaretIndex;
        private bool _selectionActive;

        public bool HasSelection => _selectionActive && SelectionLength > 0;
        public int SelectionStart { get; private set; }
        public int SelectionLength { get; private set; }
        public string SelectionValue => _input.Substring(SelectionStart, SelectionLength);
        public Color Color { get; set; }

        public void LoadContent(ConsoleInput input)
        {
            _input = input;
            _input.Caret.Moved += (s, e) =>
            {
                if (_input.ActionDefinitions.AreModifiersAppliedForAction(ConsoleAction.SelectionModifier, _input.Input))
                {
                    if (_selectionActive)
                    {
                        _selectionIndex2 = _input.Caret.Index;
                        CalculateSelectionProperties();
                    }
                    else
                    {
                        _selectionIndex1 = _previousCaretIndex;
                        _selectionIndex2 = _input.Caret.Index;
                        CalculateSelectionProperties();
                        _selectionActive |= SelectionLength > 0;
                    }
                }
                else
                {
                    Clear();
                    _previousCaretIndex = _input.Caret.Index;
                }
            };
            _input.InputChanged += (s, e) =>
            {
                Clear();
                _previousCaretIndex = _input.Caret.Index;
            };
        }

        public void Draw()
        {
            if (!HasSelection) return;

            int visibleSelectionStartIndex = Math.Max(SelectionStart, _input.VisibleStartIndex);
            int visibleEndIndex = _input.VisibleStartIndex + _input.VisibleLength - 1;
            int length = Math.Min(SelectionLength, visibleEndIndex - visibleSelectionStartIndex + 1);

            var offset = new Vector2(
                _input.Console.Padding + _input.Console.ConsoleInput.InputPrefixSize.X,
                _input.Console.WindowArea.Y + _input.Console.WindowArea.Height - _input.Console.Padding - _input.Console.FontSize.Y);

            float startX = _input.MeasureSubstring(_input.VisibleStartIndex, visibleSelectionStartIndex - _input.VisibleStartIndex).X;
            if (startX > 0)
                startX += _input.Console.Font.Spacing;
            float width = _input.MeasureSubstring(visibleSelectionStartIndex, length).X;
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
            SelectionStart = _selectionIndex1 <= _selectionIndex2 ? _selectionIndex1 : _selectionIndex2;
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
