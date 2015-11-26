using System;
using Microsoft.Xna.Framework;
using QuakeConsole.Utilities;
#if MONOGAME
using MathUtil = Microsoft.Xna.Framework.MathHelper;
#endif

namespace QuakeConsole.Input
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
                    _index = MathUtil.Clamp(value, 0, _input.Length);
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
                Vector2 position = new Vector2(
                    _input.Console.Padding + _input.InputPrefixSize.X + _input.MeasureSubstring(_input.VisibleStartIndex, Index - _input.VisibleStartIndex).X,
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
            Width = _input.Console.Font.MeasureString(Symbol).X;            
        }              
    }
}
