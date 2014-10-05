using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace Varus.Paradox.Console
{
    internal class Caret
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly Timer _caretBlinkingTimer = new Timer(0.4f) { AutoReset = true };
        private readonly SpriteFont _font;

        private bool _drawCaret;
        private string _symbol;

        public Caret(SpriteBatch spriteBatch, SpriteFont font)
        {            
            _spriteBatch = spriteBatch;
            _font = font;
            Symbol = "_";
            CalculateSymbolWidth();
        }

        public int Index { get; set; }

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
                _symbol = value;
                CalculateSymbolWidth();
            }
        }

        public float Width { get; private set; }

        public void Update(float deltaSeconds)
        {
            _caretBlinkingTimer.Update(deltaSeconds);
            if (_caretBlinkingTimer.Finished)
            {
                _drawCaret = !_drawCaret;
            }
        }

        public void Draw(ref Vector2 position, Color color)
        {
            if (_drawCaret)
            {
                _spriteBatch.DrawString(_font, Symbol, position, color);
            }
        }

        private void CalculateSymbolWidth()
        {
            Width = _font.MeasureString(Symbol).X;
        }
    }
}
