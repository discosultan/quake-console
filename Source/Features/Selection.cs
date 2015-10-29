using System;
using Microsoft.Xna.Framework;
using QuakeConsole.Utilities;

namespace QuakeConsole.Features
{
    internal class Selection
    {
        private Console _console;

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

        public void LoadContent(Console console)
        {
            _console = console;
            _console.ConsoleInput.Caret.Moved += (s, e) =>
            {
                if (!_console.ActionDefinitions.AreModifiersAppliedForAction(ConsoleAction.SelectionModifier))
                {
                    Clear();
                    _previousCaretIndex = _console.ConsoleInput.Caret.Index;
                }
            };
        } 

        private int _selectionIndex1;
        private int _selectionIndex2;        
        private int _previousCaretIndex;
        private bool _selectionActive;

        public bool HasSelection => _selectionActive && SelectionLength > 0;
        public int SelectionStart { get; private set; }
        public int SelectionLength { get; private set; }
        public string SelectionValue => _console.ConsoleInput.Substring(SelectionStart, SelectionLength);
        public Color Color { get; set; }

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;

            Caret caret = _console.ConsoleInput.Caret;
            
            switch (action)
            {
                case ConsoleAction.MoveLeft:
                case ConsoleAction.MoveRight:
                case ConsoleAction.MoveLeftWord:
                case ConsoleAction.MoveRightWord:
                case ConsoleAction.MoveToBeginning:
                case ConsoleAction.MoveToEnd:
                    if (_console.ActionDefinitions.AreModifiersAppliedForAction(ConsoleAction.SelectionModifier))
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

        public void Clear()
        {
            _selectionActive = false;
            _previousCaretIndex = 0;
            _selectionIndex1 = 0;
            _selectionIndex2 = 0;
        }

        public void OnSymbol(Symbol symbol)
        {            
            Clear();                        
            _previousCaretIndex = _console.ConsoleInput.Caret.Index;
        }

        public void Draw()
        {
            if (!HasSelection) return;

            // TODO: Fix drawing offset when input runs out of screen
            var input = _console.ConsoleInput;

            var offset = new Vector2(
                _console.Padding + _console.ConsoleInput.InputPrefixSize.X, 
                _console.WindowArea.Y + _console.WindowArea.Height - _console.Padding - _console.FontSize.Y);            
            float startX = input.MeasureSubstring(0, SelectionStart).X;
            float width = input.MeasureSubstring(SelectionStart, SelectionLength).X;
            var destRectangle = new RectangleF(
                offset.X + startX,
                offset.Y,
                width, 
                _console.FontSize.Y);
            _console.SpriteBatch.Draw(
                _console.WhiteTexture,
                destRectangle,
                Color);
        }

        private void CalculateSelectionProperties()
        {
            SelectionLength = Math.Abs(_selectionIndex2 - _selectionIndex1);
            SelectionStart = _selectionIndex1 <= _selectionIndex2 ? _selectionIndex1 : _selectionIndex2;
        }
    }
}
