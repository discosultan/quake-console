using System;
using Microsoft.Xna.Framework;

namespace QuakeConsole
{
    internal class Caret
    {
        public event EventHandler Moved;

        private readonly Timer _caretBlinkingTimer = new Timer { AutoReset = true };

        private ConsoleInput _input;

        private bool _drawCaret;
        private string _symbol;
        private int _index;
        private bool _loaded;

        public void LoadContent(ConsoleInput input)
        {
            _input = input;

            _input.Console.FontChanged += (s, e) => CalculateSymbolWidth();
            CalculateSymbolWidth();

            _loaded = true;
        }

        public int Index
        {
            get { return _index; }
            set
            {
                if (value != _index)
                {
                    _index = MathHelper.Clamp(value, 0, _input.Length);
                    Moved?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public float BlinkIntervalSeconds
        {
            get { return _caretBlinkingTimer.TargetTime; }
            set { _caretBlinkingTimer.TargetTime = value; }
        }

        public string Symbol
        {
            get { return _symbol; }
            set
            {
                Check.ArgumentNotNull(value, "value");
                _symbol = value;
                if (_loaded)
                    CalculateSymbolWidth();
            }
        }

        public float Width { get; private set; }

        public void MoveBy(int amount)
        {
            Index = Index + amount;
        }

        public void Update(float deltaSeconds)
        {
            _caretBlinkingTimer.Update(deltaSeconds);
            if (_caretBlinkingTimer.Finished)
                _drawCaret = !_drawCaret;
        }

        public void Draw()
        {
            if (_drawCaret)
            {
                float offset = _input.Console.Padding + _input.InputPrefixSize.X;
                float positionX = _input.MeasureSubstring(_input.VisibleStartIndex, Index - _input.VisibleStartIndex).X;
                if (positionX > 0)
                    positionX += _input.Console.Font.Spacing;
                var position = new Vector2(
                     offset + positionX,
                    _input.Console.WindowArea.Y + _input.Console.WindowArea.Height - _input.Console.Padding - _input.Console.FontSize.Y);
                _input.Console.SpriteBatch.DrawString(_input.Console.Font, Symbol, position, _input.Console.FontColor);
            }
        }

        public void SetSettings(ConsoleSettings settings)
        {
            Symbol = settings.CaretSymbol;
            _caretBlinkingTimer.TargetTime = settings.CaretBlinkingIntervalSeconds;
        }

        private void CalculateSymbolWidth()
        {
            Width = _input.Console.Font.MeasureString(Symbol).X + _input.Console.Font.Spacing;
        }
    }
}
