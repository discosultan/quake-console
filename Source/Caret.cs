using System;
using System.Text;
using QuakeConsole.Utilities;
#if MONOGAME
using Microsoft.Xna.Framework;
using MathUtil = Microsoft.Xna.Framework.MathHelper;
#endif

namespace QuakeConsole
{
    /// <summary>
    /// A blinking caret inside the <see cref="InputBuffer"/> to show the location of the cursor.
    /// </summary>
    public class Caret : ICaret
    {
        private readonly Console _consolePanel;
        private readonly Timer _caretBlinkingTimer = new Timer { AutoReset = true };
        private readonly StringBuilder _inputBuffer;

        private bool _drawCaret;
        private string _symbol;
        private int _index;

        internal event EventHandler Moved = delegate { };

        internal Caret(Console consolePanel, StringBuilder inputBuffer)
        {
            _consolePanel = consolePanel;
            _inputBuffer = inputBuffer;            

            consolePanel.FontChanged += (s, e) => CalculateSymbolWidth();            
        }

        /// <summary>
        /// Gets or sets the character index the cursor is at in the <see cref="InputBuffer"/>.
        /// </summary>
        public int Index
        {
            get { return _index; }
            set
            {
                _index = MathUtil.Clamp(value, 0, _inputBuffer.Length); 
                Moved(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the time in seconds to toggle visibility.
        /// </summary>
        public float BlinkIntervalSeconds
        {
            get { return _caretBlinkingTimer.TargetTime; }
            set { _caretBlinkingTimer.TargetTime = value; }
        }

        /// <summary>
        /// Gets or sets the symbol which is used as the caret.
        /// </summary>
        public string Symbol
        {
            get { return _symbol; }
            set
            {
                Check.ArgumentNotNull(value, "value");
                _symbol = value;
                CalculateSymbolWidth();
            }
        }

        internal float Width { get; private set; }

        /// <summary>
        /// Moves the caret by the specified amount of characters.
        /// </summary>
        /// <param name="amount">
        /// Amount of chars to move caret by. Positive amount will move to the right,
        /// negative to the left.
        /// </param>
        internal void MoveBy(int amount)
        {
            Index = Index + amount;            
        }

        internal void Update(float deltaSeconds)
        {
            _caretBlinkingTimer.Update(deltaSeconds);
            if (_caretBlinkingTimer.Finished)
                _drawCaret = !_drawCaret;
        }        

        internal void Draw(ref Vector2 position, Color color)
        {
            if (_drawCaret)
                _consolePanel.SpriteBatch.DrawString(_consolePanel.Font, Symbol, position, color);
        }

        private void CalculateSymbolWidth()
        {
            if (_consolePanel.Font != null)
                Width = _consolePanel.Font.MeasureString(Symbol).X;            
        }

        internal void SetDefaults(ConsoleSettings settings)
        {
            Symbol = settings.CaretSymbol;           
            _caretBlinkingTimer.TargetTime = settings.CaretBlinkingIntervalSeconds;
        }
    }
}
