using System;
using System.Text;
using SiliconStudio.Core.Mathematics;
using Varus.Paradox.Console.Utilities;

namespace Varus.Paradox.Console
{
    /// <summary>
    /// A blinking caret inside the <see cref="InputBuffer"/> to show the location of the cursor.
    /// </summary>
    public class Caret : ICaret
    {
        private readonly ConsoleShell _consolePanel;
        private readonly Timer _caretBlinkingTimer = new Timer { AutoReset = true };
        private readonly StringBuilder _inputBuffer;

        private bool _drawCaret;
        private string _symbol;

        internal event EventHandler Moved = delegate { };

        internal Caret(ConsoleShell consolePanel, StringBuilder inputBuffer)
        {
            _consolePanel = consolePanel;
            _inputBuffer = inputBuffer;            

            consolePanel.FontChanged += (s, e) => CalculateSymbolWidth();            
        }

        /// <summary>
        /// Gets or sets the index the cursor is at in the <see cref="InputBuffer"/>.
        /// </summary>
        public int Index { get; private set; }

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

        internal void Move(int amount)
        {
            Index = MathUtil.Clamp(Index + amount, 0, _inputBuffer.Length);            
            Moved(this, EventArgs.Empty);            
        }

        internal void MoveTo(int pos)
        {
            Index = pos;
            Moved(this, EventArgs.Empty);
        }

        internal void Update(float deltaSeconds)
        {
            _caretBlinkingTimer.Update(deltaSeconds);
            if (_caretBlinkingTimer.Finished)
            {
                _drawCaret = !_drawCaret;
            }
        }        

        internal void Draw(ref Vector2 position, Color color)
        {
            if (_drawCaret)
            {
                _consolePanel.SpriteBatch.DrawString(_consolePanel.Font, Symbol, position, color);
            }
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
