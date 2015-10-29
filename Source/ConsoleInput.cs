using System;
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
        private readonly SpriteFontStringBuilder _inputBuffer = new SpriteFontStringBuilder();        

        private Console _console;        

        private string _inputPrefix;        
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
        public Selection Selection { get; } = new Selection();
        public Caret Caret { get; } = new Caret();

        public int VisibleStartIndex => _startIndex;

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
                CalculateInputPrefixWidth();
                _inputBuffer.Font = _console.Font;
                _dirty = true;
            };
            _inputBuffer.Font = _console.Font;
            _console.WindowAreaChanged += (s, e) => _dirty = true;

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
            Selection.LoadContent(console);

            _loaded = true;
        }
        
        public void Append(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            _inputBuffer.Insert(Caret.Index, value);
            Caret.MoveBy(value.Length);
        }
        
        public void Remove(int startIndex, int length)
        {
            Caret.Index = startIndex;
            _inputBuffer.Remove(startIndex, length);
        }
        
        public string Value
        {
            get { return _inputBuffer.ToString(); } // Does not allocate if value is cached.
            set
            {
                ClearInput();
                if (value != null)
                    _inputBuffer.Append(value);
                Caret.Index = _inputBuffer.Length;                
            }
        }

        public Vector2 MeasureSubstring(int startIndex, int length) => _inputBuffer.MeasureSubstring(startIndex, length);

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
            ClearInput();
            Caret.MoveBy(int.MinValue);            
        }        

        public char this[int i]
        {
            get { return _inputBuffer[i]; }
            set { _inputBuffer[i] = value; }
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
            // Draw selection.
            Selection.Draw();
            // Draw input prefix.
            var inputPosition = new Vector2(_console.Padding, _console.WindowArea.Y + _console.WindowArea.Height - _console.Padding - _console.FontSize.Y);
            _console.SpriteBatch.DrawString(
                _console.Font, 
                InputPrefix, 
                inputPosition, 
                InputPrefixColor);
            // Draw input buffer.
            inputPosition.X += InputPrefixSize.X;
            if (_inputBuffer.Length > 0)
                _console.SpriteBatch.DrawString(_console.Font, _inputBuffer.Substring(_startIndex, _endIndex - _startIndex + 1), inputPosition, _console.FontColor);
            // Draw caret. 
            inputPosition.X = _console.Padding + InputPrefixSize.X + _inputBuffer.MeasureSubstring(_startIndex, Caret.Index - _startIndex).X;
            Caret.Draw(ref inputPosition, _console.FontColor);            
        }

        public void SetSettings(ConsoleSettings settings)
        {
            InputPrefix = settings.InputPrefix;
            InputPrefixColor = settings.InputPrefixColor;
            NumPositionsToMoveWhenOutOfScreen = settings.NumPositionsToMoveWhenOutOfScreen;
            RepeatingInput.RepeatingInputCooldown = settings.TimeToCooldownRepeatingInput;
            RepeatingInput.TimeUntilRepeatingInput = settings.TimeToTriggerRepeatingInput;
            Selection.Color = settings.SelectionColor;
            Selection.Enabled = settings.TextSelectionEnabled;

            Caret.SetSettings(settings);
        }

        public override string ToString() => Value;

        private void ClearInput()
        {
            Selection.Clear();
            _inputBuffer.Clear();
        }

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
            hasProcessedKey = Selection.ProcessAction(action) || hasProcessedKey;

            return hasProcessedKey;
        }

        private bool ProcessSymbolKey(Keys key)
        {
            Symbol symbol;
            if (!_console.SymbolMappings.TryGetValue(key, out symbol))
                return false;

            if (Selection.HasSelection)
                Remove(Selection.SelectionStart, Selection.SelectionLength);

            Append(CaseSenitivity.ProcessSymbol(symbol));

            InputHistory.ProcessSymbol(symbol);
            Autocompletion.ProcessSymbol(symbol);
            Selection.ProcessSymbol(symbol);

            return true;
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
