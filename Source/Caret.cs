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
    /// A blinking caret inside the <see cref="ConsoleInput"/> to show the location of the cursor.
    /// </summary>
    internal class Caret
    {
        internal event EventHandler Moved;

        private readonly Timer _caretBlinkingTimer = new Timer { AutoReset = true };

        private Console _console;
        private StringBuilder _inputBuffer;

        private bool _drawCaret;
        private string _symbol;
        private int _index;
        private bool _loaded;

        internal void LoadContent(Console console, StringBuilder inputBuffer)
        {
            _console = console;
            _inputBuffer = inputBuffer;            

            console.FontChanged += (s, e) => CalculateSymbolWidth();
            CalculateSymbolWidth();

            _loaded = true;
        }

        /// <summary>
        /// Gets or sets the character index the cursor is at in the <see cref="ConsoleInput"/>.
        /// </summary>
        public int Index
        {
            get { return _index; }
            set
            {
                _index = MathUtil.Clamp(value, 0, _inputBuffer.Length); 
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
                _console.SpriteBatch.DrawString(_console.Font, Symbol, position, color);
        }

        private void CalculateSymbolWidth()
        {            
            Width = _console.Font.MeasureString(Symbol).X;            
        }

        internal void SetDefaults(ConsoleSettings settings)
        {
            Symbol = settings.CaretSymbol;           
            _caretBlinkingTimer.TargetTime = settings.CaretBlinkingIntervalSeconds;
        }

        public void MoveToPreviousWord()
        {
            bool prevOnLetter = Index < _inputBuffer.Length && char.IsLetterOrDigit(_inputBuffer[Index]);
            for (int i = Index - 1; i >= 0; i--)
            {
                bool currentOnLetter = char.IsLetterOrDigit(_inputBuffer[i]);                
                if (prevOnLetter && !currentOnLetter && i != Index - 1)
                {
                    Index = i + 1;
                    return;
                }
                prevOnLetter = currentOnLetter;
            }
            Index = 0;
        }

        public void MoveToNextWord()
        {
            bool prevOnLetter = Index < _inputBuffer.Length && char.IsLetterOrDigit(_inputBuffer[Index]);
            for (int i = Index + 1; i < _inputBuffer.Length; i++)
            {
                bool currentOnLetter = char.IsLetterOrDigit(_inputBuffer[i]);
                if (!prevOnLetter && currentOnLetter)
                {
                    Index = i;
                    return;
                }
                prevOnLetter = currentOnLetter;
            }
            Index = _inputBuffer.Length;
        }
    }
}
