using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using QuakeConsole.Input.Features;
using QuakeConsole.Utilities;
#if MONOGAME
using MathUtil = Microsoft.Xna.Framework.MathHelper;
#endif

namespace QuakeConsole.Input
{
    internal partial class ConsoleInput : IConsoleInput
    {
        public event EventHandler Cleared;

        private readonly SpriteFontStringBuilder _inputBuffer = new SpriteFontStringBuilder();        

        private string _inputPrefix;        
        private int _numPosToMoveWhenOutOfScreen;
        private bool _dirty;

        private bool _loaded;

        public Console Console { get; private set; }
#if MONOGAME
        public InputState Input { get; } = new InputState();
#endif
        public Caret Caret { get; } = new Caret();
        public RepeatingInput RepeatingInput { get; } = new RepeatingInput();
        public InputHistory InputHistory { get; } = new InputHistory();
        public Autocompletion Autocompletion { get; } = new Autocompletion();
        public CopyPasting CopyPasting { get; } = new CopyPasting();
        public Movement Movement { get; } = new Movement();
        public Tabbing Tabbing { get; } = new Tabbing();
        public Deletion Deletion { get; } = new Deletion();
        public CommandExecution CommandExecution { get; } = new CommandExecution();
        public CaseSensitivity CaseSenitivity { get; } = new CaseSensitivity();
        public Selection Selection { get; } = new Selection();
        public MultiLineInput MultiLineInput { get; } = new MultiLineInput();

        public Dictionary<Keys, Symbol> SymbolMappings
        {
            get { return _symbolDefinitions; }
            set { _symbolDefinitions = value ?? new Dictionary<Keys, Symbol>(); }
        }

        public int VisibleStartIndex { get; private set; }
        public int VisibleLength { get; private set; }

        public string LastAutocompleteEntry
        {
            get { return Autocompletion.LastAutocompleteEntry; }
            set { Autocompletion.LastAutocompleteEntry = value; }
        }        

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

        public Vector2 InputPrefixSize { get; set; }

        public void LoadContent(Console console)
        {
            Console = console;
            Console.FontChanged += (s, e) =>
            {
                CalculateInputPrefixWidth();
                _inputBuffer.Font = Console.Font;
                _dirty = true;
            };
            _inputBuffer.Font = Console.Font;
            Console.WindowAreaChanged += (s, e) => _dirty = true;

            CalculateInputPrefixWidth();

            Caret.LoadContent(Console);
            Caret.Moved += (s, e) => _dirty = true;

            RepeatingInput.LoadContent(this);
            InputHistory.LoadContent(this);
            Autocompletion.LoadContent(this);
            CopyPasting.LoadContent(this);
            Movement.LoadContent(this);
            Tabbing.LoadContent(this);
            Deletion.LoadContent(this);
            CommandExecution.LoadContent(this);
            CaseSenitivity.LoadContent(this);
            Selection.LoadContent(this);
            MultiLineInput.LoadContent(this);

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
                Clear();
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
            _inputBuffer.Clear();
            Caret.MoveBy(int.MinValue);
            Cleared?.Invoke(this, EventArgs.Empty);
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
            ProcessInput(Input);

            Caret.Update(deltaSeconds);
            RepeatingInput.Update(deltaSeconds);
            if (_dirty)
            {
                CalculateStartAndEndIndices();
                _dirty = false;
            }
        }

        public void ProcessInput(InputState input)
        {
            ConsoleAction action;
            if (ActionDefinitions.TryGetAction(input, out action))
            {
                ProcessAction(action);
            }
            else
            {
                for (int i = 0; i < input.PressedKeys.Count; i++)
                {
                    Keys key = input.PressedKeys[i];
                    Symbol symbol;
                    if (SymbolMappings.TryGetValue(key, out symbol))
                        ProcessSymbol(symbol);
                }
            }
        }

        public void ProcessAction(ConsoleAction action)
        {
            InputHistory.OnAction(action);
            Autocompletion.OnAction(action);
            CopyPasting.OnAction(action);
            Movement.OnAction(action);
            Tabbing.OnAction(action);
            Deletion.OnAction(action);
            CommandExecution.OnAction(action);
            CaseSenitivity.OnAction(action);
            Selection.OnAction(action);
            MultiLineInput.OnAction(action);
            RepeatingInput.OnAction(action);
        }

        public void ProcessSymbol(Symbol symbol)
        {
            if (Selection.HasSelection)
                Remove(Selection.SelectionStart, Selection.SelectionLength);

            Append(CaseSenitivity.ProcessSymbol(symbol));

            InputHistory.OnSymbol(symbol);
            Autocompletion.OnSymbol(symbol);
            Selection.OnSymbol(symbol);
            RepeatingInput.OnSymbol(symbol);
        }

        public void Draw()
        {            
            // Draw selection.
            Selection.Draw();
            // Draw input prefix.
            var inputPosition = new Vector2(Console.Padding, Console.WindowArea.Y + Console.WindowArea.Height - Console.Padding - Console.FontSize.Y);
            Console.SpriteBatch.DrawString(
                Console.Font, 
                InputPrefix, 
                inputPosition, 
                InputPrefixColor);
            // Draw input buffer.
            inputPosition.X += InputPrefixSize.X;
            if (_inputBuffer.Length > 0)
                Console.SpriteBatch.DrawString(Console.Font, _inputBuffer.Substring(VisibleStartIndex, VisibleLength), inputPosition, Console.FontColor);
            // Draw caret. 
            inputPosition.X = Console.Padding + InputPrefixSize.X + _inputBuffer.MeasureSubstring(VisibleStartIndex, Caret.Index - VisibleStartIndex).X;
            Caret.Draw(ref inputPosition, Console.FontColor);            
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

        private void CalculateInputPrefixWidth()
        {
            InputPrefixSize = Console.Font.MeasureString(InputPrefix);
        }

        private void CalculateStartAndEndIndices()
        {
            float windowWidth = Console.WindowArea.Width - Console.Padding * 2 - InputPrefixSize.X;

            if (Caret.Index > _inputBuffer.Length - 1)
                windowWidth -= Caret.Width;

            while (Caret.Index <= VisibleStartIndex && VisibleStartIndex > 0)
                VisibleStartIndex = Math.Max(VisibleStartIndex - NumPositionsToMoveWhenOutOfScreen, 0);

            VisibleLength = MathUtil.Min(VisibleLength, _inputBuffer.Length - VisibleStartIndex - 1);

            float widthProgress = 0f;
            int indexer = VisibleStartIndex;
            int targetIndex = Caret.Index;            
            while (indexer < _inputBuffer.Length)
            {
                char c = _inputBuffer[indexer++];

                float charWidth;
                if (!Console.CharWidthMap.TryGetValue(c, out charWidth))
                {
                    charWidth = Console.Font.MeasureString(c.ToString()).X;
                    Console.CharWidthMap.Add(c, charWidth);
                }
                
                widthProgress += charWidth;

                if (widthProgress > windowWidth)
                {                    
                    if (targetIndex >= VisibleStartIndex && targetIndex - VisibleStartIndex < VisibleLength || indexer - 1 == VisibleStartIndex) break;

                    if (targetIndex >= VisibleStartIndex)
                    {
                        VisibleStartIndex += NumPositionsToMoveWhenOutOfScreen;
                        VisibleStartIndex = Math.Min(VisibleStartIndex, _inputBuffer.Length - 1);
                    }
                    CalculateStartAndEndIndices();
                    break;
                }

                VisibleLength = indexer - VisibleStartIndex;
            }
        }
    }
}
