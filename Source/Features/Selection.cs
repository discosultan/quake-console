using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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

        public void LoadContent(Console console) => _console = console;

        private int _selectionIndex1;
        private int _selectionIndex2;        
        private int _previousCaretIndex;
        private bool _selectionActive;

        public bool HasSelection => _selectionActive && SelectionLength > 0;
        public int SelectionStart { get; private set; }
        public int SelectionLength { get; private set; }
        public string SelectionValue => _console.ConsoleInput.Substring(SelectionStart, SelectionLength);
        public Color Color { get; set; }

        public bool ProcessAction(ConsoleAction action)
        {
            if (!Enabled) return false;

            ConsoleInput input = _console.ConsoleInput;
            Caret caret = _console.ConsoleInput.Caret;
            
            switch (action)
            {
                case ConsoleAction.MoveLeft:
                case ConsoleAction.MoveRight:
                case ConsoleAction.MoveToBeginning:
                case ConsoleAction.MoveToEnd:
                    Keys modifier;
                    _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.SelectionModifier, out modifier);
                    if (input.Input.IsKeyDown(modifier))
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
                case ConsoleAction.Autocomplete:
                case ConsoleAction.ExecuteCommand:
                case ConsoleAction.Tab:
                case ConsoleAction.DeleteCurrentChar:
                case ConsoleAction.DeletePreviousChar:
                case ConsoleAction.Paste:
                case ConsoleAction.Cut:
                case ConsoleAction.NextCommandInHistory:
                case ConsoleAction.PreviousCommandInHistory:                  
                    Clear();
                    _previousCaretIndex = caret.Index;
                    break;
            }
            return false;
        }

        public void Clear()
        {
            _selectionActive = false;
            _previousCaretIndex = 0;
            _selectionIndex1 = 0;
            _selectionIndex2 = 0;
        }

        public void ProcessSymbol(Symbol symbol)
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
