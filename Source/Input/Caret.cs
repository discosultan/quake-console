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

        private Console _console;

        private bool _drawCaret;
        private string _symbol;
        private int _index;
        private bool _loaded;

        public void LoadContent(Console console)
        {
            _console = console;

            console.FontChanged += (s, e) => CalculateSymbolWidth();
            CalculateSymbolWidth();

            _loaded = true;
        }

        public int Index
        {
            get { return _index; }
            set
            {
                _index = MathUtil.Clamp(value, 0, _console.ConsoleInput.Length); 
                Moved?.Invoke(this, EventArgs.Empty);
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

        public void Draw(ref Vector2 position, Color color)
        {
            if (_drawCaret)
                _console.SpriteBatch.DrawString(_console.Font, Symbol, position, color);
        }

        public void SetSettings(ConsoleSettings settings)
        {
            Symbol = settings.CaretSymbol;
            _caretBlinkingTimer.TargetTime = settings.CaretBlinkingIntervalSeconds;
        }

        private void CalculateSymbolWidth()
        {            
            Width = _console.Font.MeasureString(Symbol).X;            
        }              
    }
}
