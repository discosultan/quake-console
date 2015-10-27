using System;
using System.Text;
using Microsoft.Xna.Framework.Input;
using QuakeConsole.Features;
using QuakeConsole.Utilities;
#if MONOGAME
using Microsoft.Xna.Framework;
using MathUtil = Microsoft.Xna.Framework.MathHelper;
#endif

namespace QuakeConsole
{
    internal class ConsoleInput : IConsoleInput
    {        
        private readonly StringBuilder _inputBuffer = new StringBuilder();
        private readonly StringBuilder _drawBuffer = new StringBuilder(); // Helper buffer for drawing to avoid unnecessary string allocations.      

        private Console _console;

        private string _value;
        private bool _valueDirty = true;

        private string _inputPrefix;
        private Vector2 _fontSize;
        private int _startIndex;
        private int _endIndex;        
        private int _numPosToMoveWhenOutOfScreen;
        private bool _dirty;

        private bool _loaded;

        public RepeatingInput RepeatingInput { get; } = new RepeatingInput();

        public string LastAutocompleteEntry
        {
            get { return Autocompletion.LastAutocompleteEntry; }
            set { Autocompletion.LastAutocompleteEntry = value; }
        }

        public InputHistory InputHistory { get; } = new InputHistory();
        public Autocompletion Autocompletion { get; } = new Autocompletion();
        public CopyPasting CopyPasting { get; } = new CopyPasting();
        public Movement Movement { get; } = new Movement();
        public Tabbing Tabbing { get; } = new Tabbing();
        public Deletion Deletion { get; } = new Deletion();
        public CommandExecution CommandExecution { get; } = new CommandExecution();
        public CaseSensitivity CaseSenitivity { get; } = new CaseSensitivity();
        public Caret Caret { get; } = new Caret();

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

        public int Length => _inputBuffer.Length;

        public int NumPositionsToMoveWhenOutOfScreen
        {
            get { return _numPosToMoveWhenOutOfScreen; }
            set { _numPosToMoveWhenOutOfScreen = Math.Max(value, 1); }
        }


#if MONOGAME
        public InputManager Input { get; } = new InputManager();
#endif
        public Vector2 InputPrefixSize { get; set; }

        public void LoadContent(Console console)
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

            Caret.LoadContent(_console);
            Caret.Moved += (s, e) => _dirty = true;

            RepeatingInput.LoadContent(console);
            InputHistory.LoadContent(console);
            Autocompletion.LoadContent(console);
            CopyPasting.LoadContent(console);
            Movement.LoadContent(console);
            Tabbing.LoadContent(console);
            Deletion.LoadContent(console);
            CommandExecution.LoadContent(console);
            CaseSenitivity.LoadContent(console);

            _loaded = true;
        }
        
        public void Append(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            _inputBuffer.Insert(Caret.Index, value);
            _valueDirty = true;
            Caret.MoveBy(value.Length);
        }
        
        public void Remove(int startIndex, int length)
        {
            Caret.Index = startIndex;
            _inputBuffer.Remove(startIndex, length);
            _valueDirty = true;                  
        }
        
        public string Value
        {
            get
            {                
                if (_valueDirty)
                {
                    _valueDirty = false;
                    _value = _inputBuffer.ToString();
                }
                return _value;
            }
            set
            {
                _inputBuffer.Clear();
                _valueDirty = true;
                if (value != null)
                    _inputBuffer.Append(value);
                Caret.Index = _inputBuffer.Length;                
            }
        }

        public string Substring(int startIndex, int length)
        {            
            return _inputBuffer.Substring(startIndex, length);
        }

        public string Substring(int startIndex)
        {            
            return _inputBuffer.Substring(startIndex);
        }

        public void Clear()
        {
            _inputBuffer.Clear();
            _valueDirty = true;
            Caret.MoveBy(int.MinValue);            
        }        

        public char this[int i]
        {
            get { return _inputBuffer[i]; }
            set
            {
                _inputBuffer[i] = value;
                _valueDirty = true;
            }
        }                

        public void Update(float deltaSeconds)
        {
#if MONOGAME
            Input.Update();
#endif
            foreach (KeyEvent keyEvent in Input.KeyEvents)
                // We are only interested in key presses.
                if (keyEvent.Type == KeyEventType.Pressed)
                    if (HandleKey(keyEvent.Key))
                        break;
            Caret.Update(deltaSeconds);
            RepeatingInput.Update(deltaSeconds);
            if (_dirty)
            {
                CalculateStartAndEndIndices();
                _dirty = false;
            }
        }

        public bool HandleKey(Keys key)
        {
            bool processedKey = ProcessActionKey(key);
            if (processedKey)
                return true;
            processedKey = ProcessSymbolKey(key);
            return processedKey;
        }

        public void Draw()
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

        public void SetSettings(ConsoleSettings settings)
        {
            InputPrefix = settings.InputPrefix;
            InputPrefixColor = settings.InputPrefixColor;
            NumPositionsToMoveWhenOutOfScreen = settings.NumPositionsToMoveWhenOutOfScreen;
            RepeatingInput.RepeatingInputCooldown = settings.TimeToCooldownRepeatingInput;
            RepeatingInput.TimeUntilRepeatingInput = settings.TimeToTriggerRepeatingInput;

            Caret.SetSettings(settings);
        }

        public override string ToString() => Value;

        private bool ProcessActionKey(Keys key)
        {
            ConsoleAction action;
            if (!_console.ActionDefinitions.ForwardTryGetValue(key, out action))
                return false;

            bool hasProcessedKey = InputHistory.ProcessAction(action);
            hasProcessedKey = Autocompletion.ProcessAction(action) || hasProcessedKey;
            hasProcessedKey = CopyPasting.ProcessAction(action) || hasProcessedKey;
            hasProcessedKey = Movement.ProcessAction(action) || hasProcessedKey;
            hasProcessedKey = Tabbing.ProcessAction(action) || hasProcessedKey;
            hasProcessedKey = Deletion.ProcessAction(action) || hasProcessedKey;
            hasProcessedKey = CommandExecution.ProcessAction(action) || hasProcessedKey;
            hasProcessedKey = CaseSenitivity.ProcessAction(action) || hasProcessedKey;       

            return hasProcessedKey;
        }

        private bool ProcessSymbolKey(Keys key)
        {
            Symbol symbol;
            if (!_console.SymbolMappings.TryGetValue(key, out symbol))
                return false;

            InputHistory.ProcessSymbol(symbol);
            Autocompletion.ProcessSymbol(symbol);

            Append(CaseSenitivity.ProcessSymbol(symbol));

            return true;
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
