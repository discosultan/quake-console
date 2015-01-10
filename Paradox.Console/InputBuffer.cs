using System;
using System.Text;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using Varus.Paradox.Console.Utilities;

namespace Varus.Paradox.Console
{
    /// <summary>
    /// Input part of the <see cref="ConsoleShell"/>. User input, historical commands and autocompletion values will be appended here.
    /// </summary>
    public class InputBuffer
    {
        private readonly ConsoleShell _consolePanel;
        private Vector2 _fontSize;
        private string _inputPrefix;                
        private int _startIndex;
        private int _endIndex;
        private readonly StringBuilder _buffer = new StringBuilder();
        private int _numPosToMoveWhenOutOfScreen;
        private bool _dirty;
        private readonly StringBuilder _drawBuffer = new StringBuilder();

        internal InputBuffer(ConsoleShell consolePanel)
        {
            _consolePanel = consolePanel;
            _consolePanel.FontChanged += (s, e) =>
            {
                MeasureFontSize();
                CalculateInputPrefixWidth();                
                _dirty = true;
            };
            _consolePanel.WindowAreaChanged += (s, e) => _dirty = true;            
            Caret = new Caret(consolePanel, _buffer);
            Caret.Moved += (s, e) => _dirty = true;            

            MeasureFontSize();
        }

        internal Vector2 InputPrefixSize { get; set; }

        /// <summary>
        /// Gets or sets the last autocomplete entry which was added to the buffer. Note that
        /// this value will be set to null whenever anything from the normal <see cref="ConsoleShell"/>
        /// input pipeline gets appended here.
        /// </summary>
        public string LastAutocompleteEntry { get; set; }
        /// <summary>
        /// Gets the <see cref="Caret"/> associated with the buffer. This indicates where user input will be appended.
        /// </summary>
        public Caret Caret { get; private set; }
        /// <summary>
        /// Gets or sets the symbol that is shown in the beginning of the <see cref="InputBuffer"/>.
        /// </summary>
        public string InputPrefix
        {
            get { return _inputPrefix; }
            set
            {
                value = value ?? "";
                _inputPrefix = value;
                CalculateInputPrefixWidth();
                _dirty = true;
            }
        }
        /// <summary>
        /// Gets or sets the color for the input prefix symbol.
        /// </summary>
        public Color InputPrefixColor { get; set; }
        /// <summary>
        /// Gets or sets the number of characters currently in the buffer.
        /// </summary>
        public int Length
        {
            get { return _buffer.Length; }
        }
        /// <summary>
        /// Gets or sets the time in seconds it takes to append a new symbol in case user is holding down a key
        /// and repeating input has been activated.
        /// </summary>
        public float RepeatingInputCooldown
        {
            get { return _consolePanel.RepeatingInputCooldown; }
            set
            {
                Check.ArgumentNotLessThan(value, 0, "value");
                _consolePanel.RepeatingInputCooldown = value;
            }
        }
        /// <summary>
        /// Gets or sets the time in seconds it takes after user started holding down a key to enable repeating input.
        /// Repeating input means that the keys hold down will be processed repeatedly without having to repress the keys.
        /// </summary>
        public float TimeUntilRepeatingInput
        {
            get { return _consolePanel.TimeUntilRepeatingInput; }
            set
            {
                Check.ArgumentNotLessThan(value, 0, "value");
                _consolePanel.TimeUntilRepeatingInput = value;
            }
        } 
        /// <summary>
        /// Gets or sets the number of symbols that will be brought into <see cref="InputBuffer"/> view once the user moves
        /// <see cref="Caret"/> out of the visible area.
        /// </summary>
        public int NumPositionsToMoveWhenOutOfScreen
        {
            get { return _numPosToMoveWhenOutOfScreen; }
            set
            {
                Check.ArgumentNotLessThan(value, 1, "value");
                _numPosToMoveWhenOutOfScreen = value;
            }
        }
        /// <summary>
        /// Writes symbol to the <see cref="InputBuffer"/>.
        /// </summary>
        /// <param name="symbol">Symbol to write.</param>
        public void Write(string symbol)
        {
            if (string.IsNullOrEmpty(symbol)) return;            
            _buffer.Insert(Caret.Index, symbol);            
            Caret.Move(symbol.Length);            
        }
        /// <summary>
        /// Removes symbols from the <see cref="InputBuffer"/>.
        /// </summary>
        /// <param name="startIndex">Index from which to start removing.</param>
        /// <param name="length">Number of symbols to remove.</param>
        public void Remove(int startIndex, int length)
        {
            //Caret.Move(-length);
            Caret.MoveTo(startIndex);
            _buffer.Remove(startIndex, length);                       
        }
        internal void RemoveTab()
        {
            bool isTab = true;
            int counter = 0;
            for (int i = Caret.Index - 1; i >= 0; i--)
            {                
                if (counter >= _consolePanel.Tab.Length) break;
                if (_buffer[i] != _consolePanel.Tab[_consolePanel.Tab.Length - counter++ - 1])
                {
                    isTab = false;
                    break;
                }
            }
            int numToRemove = counter;
            if (isTab)
            {
                _buffer.Remove(Math.Max(0, Caret.Index - _consolePanel.Tab.Length), numToRemove);
            }
            Caret.Move(-_consolePanel.Tab.Length);
        }
        /// <summary>
        /// Sets the value typed into the buffer.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public void Set(string value)
        {
            _buffer.Clear();
            if (value != null) _buffer.Append(value);
            Caret.MoveTo(_buffer.Length);            
        }
        /// <summary>
        /// Gets the current value typed into the buffer.
        /// </summary>
        /// <returns>Value of <see cref="InputBuffer"/>.</returns>
        public string Get()
        {
            return _buffer.ToString();
        }
        /// <summary>
        /// Gets a substring of the buffer.
        /// </summary>
        /// <param name="startIndex">Index ta take substring from.</param>
        /// <param name="length">Number of symbols to include in the substring.</param>
        /// <returns>Substring of the buffer.</returns>
        public string Substring(int startIndex, int length)
        {            
            return _buffer.Substring(startIndex, length);
        }
        /// <summary>
        /// Gets a substring of the buffer.
        /// </summary>
        /// <param name="startIndex">Index ta take all the following symbols from.</param>        
        /// <returns>Substring of the buffer.</returns>
        public string Substring(int startIndex)
        {            
            return _buffer.Substring(startIndex);
        }
        /// <summary>
        /// Gets or sets if the buffer is empty or contains only whitespace symbols.
        /// </summary>
        /// <returns>True if empty or contains only whitespace(s).</returns>
        internal bool IsEmptyOrWhitespace()
        {
            return _buffer.IsEmptyOrWhitespace();
        }
        /// <summary>
        /// Clears the input from the buffer.
        /// </summary>
        public void Clear()
        {
            _buffer.Clear();
            Caret.Move(int.MinValue);         
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
            var inputPosition = new Vector2(_consolePanel.Padding, _consolePanel.WindowArea.Y + _consolePanel.WindowArea.Height - _consolePanel.Padding - _fontSize.Y);
            _consolePanel.SpriteBatch.DrawString(
                _consolePanel.Font, 
                InputPrefix, 
                inputPosition, 
                InputPrefixColor);
            // Draw input buffer.
            inputPosition.X += InputPrefixSize.X;
            if (_buffer.Length > 0)
            {                
                _buffer.ClearAndCopyTo(_drawBuffer, _startIndex, _endIndex - _startIndex + 1);
                _consolePanel.SpriteBatch.DrawString(_consolePanel.Font, _drawBuffer, inputPosition, _consolePanel.FontColor);
            }
            // Draw caret. 
            _buffer.ClearAndCopyTo(_drawBuffer, _startIndex, Caret.Index - _startIndex);
            inputPosition.X = _consolePanel.Padding + InputPrefixSize.X + _consolePanel.Font.MeasureString(_drawBuffer).X;
            Caret.Draw(ref inputPosition, _consolePanel.FontColor);            
        }

        /// <summary>
        /// Gets the symbol at specified index.
        /// </summary>
        /// <param name="i">Index to take symbol from.</param>
        /// <returns>Indexed symbol.</returns>
        public char this[int i]
        { 
            get { return _buffer[i]; }
        }

        private void MeasureFontSize()
        {
            _fontSize = _consolePanel.Font.MeasureString("x");
        }

        private void CalculateInputPrefixWidth()
        {            
            InputPrefixSize = _consolePanel.Font.MeasureString(InputPrefix);
        }

        private void CalculateStartAndEndIndices()
        {
            float windowWidth = _consolePanel.WindowArea.Width - _consolePanel.Padding * 2 - InputPrefixSize.X;

            if (Caret.Index > _buffer.Length - 1)
                windowWidth -= Caret.Width;

            while (Caret.Index < _startIndex)
            {
                _startIndex = Math.Max(_startIndex - NumPositionsToMoveWhenOutOfScreen, 0);
            }
                        
            _endIndex = Math.Max(_endIndex, Caret.Index);
            _endIndex = Math.Min(_endIndex, _buffer.Length - 1);            

            float widthProgress = 0f;
            int indexer = _startIndex;
            int targetIndex = Caret.Index;            
            while (indexer < _buffer.Length)
            {
                char c = _buffer[indexer++];

                float charWidth;
                if (!_consolePanel.CharWidthMap.TryGetValue(c, out charWidth))
                {
                    charWidth = _consolePanel.Font.MeasureString(c.ToString()).X;
                    _consolePanel.CharWidthMap.Add(c, charWidth);
                }
                
                widthProgress += charWidth;

                if (widthProgress > windowWidth)
                {                    
                    if (targetIndex >= _startIndex && targetIndex <= _endIndex || indexer - 1 == _startIndex) break;

                    if (targetIndex >= _startIndex)
                    {
                        _startIndex += NumPositionsToMoveWhenOutOfScreen;
                        _startIndex = Math.Min(_startIndex, _buffer.Length - 1);
                    }
                    CalculateStartAndEndIndices();
                    break;
                }

                _endIndex = indexer - 1;
            }
        }

        internal void SetDefaults(ConsoleSettings settings)
        {
            InputPrefix = settings.InputPrefix;
            InputPrefixColor = settings.InputPrefixColor;
            NumPositionsToMoveWhenOutOfScreen = settings.NumPositionsToMoveWhenOutOfScreen;

            Caret.SetDefaults(settings);
        }
        
        /// <inheritdoc/>
        public override string ToString()
        {
            return _buffer.ToString();
        }
    }
}
