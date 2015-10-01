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
    /// Input part of the <see cref="Console"/>. User input, historical commands and autocompletion values will be appended here.
    /// </summary>
    internal class ConsoleInput : IConsoleInput
    {        
        private readonly StringBuilder _inputBuffer = new StringBuilder();
        private readonly StringBuilder _drawBuffer = new StringBuilder(); // Helper buffer for drawing to avoid unnecessary string allocations.      

        private Console _console;

        private string _inputPrefix;
        private Vector2 _fontSize;
        private int _startIndex;
        private int _endIndex;        
        private int _numPosToMoveWhenOutOfScreen;
        private bool _dirty;

        private bool _loaded;

        internal void LoadContent(Console console)
        {
            _console = console;
            _console.FontChanged += (s, e) =>
            {
                MeasureFontSize();
                CalculateInputPrefixWidth();
                _dirty = true;
            };
            _console.WindowAreaChanged += (s, e) => _dirty = true;            

            MeasureFontSize();
            CalculateInputPrefixWidth();

            Caret.LoadContent(_console, _inputBuffer);
            Caret.Moved += (s, e) => _dirty = true;

            _loaded = true;
        }

        /// <summary>
        /// Gets or sets the last autocomplete entry which was added to the buffer. Note that
        /// this value will be set to null whenever anything from the normal <see cref="Console"/>
        /// input pipeline gets appended here.
        /// </summary>
        public string LastAutocompleteEntry { get; set; }

        /// <summary>
        /// Gets the <see cref="Caret"/> associated with the buffer. This indicates where user input will be appended.
        /// </summary>
        public Caret Caret { get; } = new Caret();

        /// <summary>
        /// Gets or sets the location of caret. This indicates where user input will be appended.
        /// </summary>
        public int CaretIndex
        {
            get { return Caret.Index; }
            set { Caret.Index = value; }
        }
        
        public string InputPrefix
        {
            get { return _inputPrefix; }
            set
            {
                value = value ?? "";
                _inputPrefix = value;
                if (_loaded)
                    CalculateInputPrefixWidth();
                _dirty = true;
            }
        }
        
        public Color InputPrefixColor { get; set; }

        /// <summary>
        /// Gets the number of characters currently in the buffer.
        /// </summary>
        public int Length => _inputBuffer.Length;

        public float RepeatingInputCooldown
        {
            get { return _console.RepeatingInputCooldown; }
            set
            {
                Check.ArgumentNotLessThan(value, 0, "value");
                _console.RepeatingInputCooldown = value;
            }
        }

        public float TimeUntilRepeatingInput
        {
            get { return _console.TimeUntilRepeatingInput; }
            set
            {
                Check.ArgumentNotLessThan(value, 0, "value");
                _console.TimeUntilRepeatingInput = value;
            }
        } 

        public int NumPositionsToMoveWhenOutOfScreen
        {
            get { return _numPosToMoveWhenOutOfScreen; }
            set
            {
                Check.ArgumentNotLessThan(value, 1, "value");
                _numPosToMoveWhenOutOfScreen = value;
            }
        }

        internal Vector2 InputPrefixSize { get; set; }

        /// <summary>
        /// Writes symbol to the <see cref="ConsoleInput"/>.
        /// </summary>
        /// <param name="symbol">Symbol to write.</param>
        public void Write(string symbol)
        {
            if (string.IsNullOrEmpty(symbol)) return;
            _inputBuffer.Insert(Caret.Index, symbol);            
            Caret.MoveBy(symbol.Length);            
        }

        /// <summary>
        /// Removes symbols from the <see cref="ConsoleInput"/>.
        /// </summary>
        /// <param name="startIndex">Index from which to start removing.</param>
        /// <param name="length">Number of symbols to remove.</param>
        public void Remove(int startIndex, int length)
        {
            //Caret.Move(-length);
            Caret.Index = startIndex;
            _inputBuffer.Remove(startIndex, length);                       
        }

        /// <summary>
        /// Gets or sets the value typed into the buffer.
        /// </summary>
        public string Value
        {
            get { return _inputBuffer.ToString(); }
            set
            {
                _inputBuffer.Clear();
                if (value != null) _inputBuffer.Append(value);
                Caret.Index = _inputBuffer.Length;
            }
        }

        /// <summary>
        /// Gets a substring of the buffer.
        /// </summary>
        /// <param name="startIndex">Index ta take substring from.</param>
        /// <param name="length">Number of symbols to include in the substring.</param>
        /// <returns>Substring of the buffer.</returns>
        public string Substring(int startIndex, int length)
        {            
            return _inputBuffer.Substring(startIndex, length);
        }

        /// <summary>
        /// Gets a substring of the buffer.
        /// </summary>
        /// <param name="startIndex">Index ta take all the following symbols from.</param>        
        /// <returns>Substring of the buffer.</returns>
        public string Substring(int startIndex)
        {            
            return _inputBuffer.Substring(startIndex);
        }

        /// <summary>
        /// Clears the input from the buffer.
        /// </summary>
        public void Clear()
        {
            _inputBuffer.Clear();
            Caret.MoveBy(int.MinValue);
        }        

        /// <inheritdoc/>
        public override string ToString()
        {
            return _inputBuffer.ToString();
        }

        /// <summary>
        /// Gets the symbol at the specified index.
        /// </summary>
        /// <param name="i">Index to take symbol from.</param>
        /// <returns>Indexed symbol.</returns>
        public char this[int i] => _inputBuffer[i];

        internal void RemoveTab()
        {
            bool isTab = true;
            int counter = 0;
            for (int i = Caret.Index - 1; i >= 0; i--)
            {
                if (counter >= _console.Tab.Length) break;
                if (_inputBuffer[i] != _console.Tab[_console.Tab.Length - counter++ - 1])
                {
                    isTab = false;
                    break;
                }
            }
            int numToRemove = counter;
            if (isTab)
            {
                _inputBuffer.Remove(Math.Max(0, Caret.Index - _console.Tab.Length), numToRemove);
            }
            Caret.MoveBy(-_console.Tab.Length);
        }

        /// <summary>
        /// Gets or sets if the buffer is empty or contains only whitespace symbols.
        /// </summary>
        /// <returns>True if empty or contains only whitespace(s).</returns>
        internal bool IsEmptyOrWhitespace()
        {
            return _inputBuffer.IsEmptyOrWhitespace();
        }        

        internal void Update(float deltaSeconds)
        {
            Caret.Update(deltaSeconds);
            if (_dirty)
            {
                CalculateStartAndEndIndices();
                _dirty = false;
            }
        }

        internal void Draw()
        {            
            // Draw input prefix.
            var inputPosition = new Vector2(_console.Padding, _console.WindowArea.Y + _console.WindowArea.Height - _console.Padding - _fontSize.Y);
            _console.SpriteBatch.DrawString(
                _console.Font, 
                InputPrefix, 
                inputPosition, 
                InputPrefixColor);
            // Draw input buffer.
            inputPosition.X += InputPrefixSize.X;
            if (_inputBuffer.Length > 0)
            {                
                _inputBuffer.ClearAndCopyTo(_drawBuffer, _startIndex, _endIndex - _startIndex + 1);
                _console.SpriteBatch.DrawString(_console.Font, _drawBuffer, inputPosition, _console.FontColor);
            }
            // Draw caret. 
            _inputBuffer.ClearAndCopyTo(_drawBuffer, _startIndex, Caret.Index - _startIndex);
            inputPosition.X = _console.Padding + InputPrefixSize.X + _console.Font.MeasureString(_drawBuffer).X;
            Caret.Draw(ref inputPosition, _console.FontColor);            
        }

        internal void SetDefaults(ConsoleSettings settings)
        {
            InputPrefix = settings.InputPrefix;
            InputPrefixColor = settings.InputPrefixColor;
            NumPositionsToMoveWhenOutOfScreen = settings.NumPositionsToMoveWhenOutOfScreen;

            Caret.SetDefaults(settings);
        }

        private void MeasureFontSize()
        {
            _fontSize = _console.Font.MeasureString("x");
        }

        private void CalculateInputPrefixWidth()
        {            
            InputPrefixSize = _console.Font.MeasureString(InputPrefix);
        }

        private void CalculateStartAndEndIndices()
        {
            float windowWidth = _console.WindowArea.Width - _console.Padding * 2 - InputPrefixSize.X;

            if (Caret.Index > _inputBuffer.Length - 1)
                windowWidth -= Caret.Width;

            while (Caret.Index <= _startIndex && _startIndex > 0)
                _startIndex = Math.Max(_startIndex - NumPositionsToMoveWhenOutOfScreen, 0);
                                    
            _endIndex = MathUtil.Clamp(_endIndex, Caret.Index, _inputBuffer.Length - 1);

            float widthProgress = 0f;
            int indexer = _startIndex;
            int targetIndex = Caret.Index;            
            while (indexer < _inputBuffer.Length)
            {
                char c = _inputBuffer[indexer++];

                float charWidth;
                if (!_console.CharWidthMap.TryGetValue(c, out charWidth))
                {
                    charWidth = _console.Font.MeasureString(c.ToString()).X;
                    _console.CharWidthMap.Add(c, charWidth);
                }
                
                widthProgress += charWidth;

                if (widthProgress > windowWidth)
                {                    
                    if (targetIndex >= _startIndex && targetIndex <= _endIndex || indexer - 1 == _startIndex) break;

                    if (targetIndex >= _startIndex)
                    {
                        _startIndex += NumPositionsToMoveWhenOutOfScreen;
                        _startIndex = Math.Min(_startIndex, _inputBuffer.Length - 1);
                    }
                    CalculateStartAndEndIndices();
                    break;
                }

                _endIndex = indexer - 1;
            }
        }
    }
}
