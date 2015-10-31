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
        public event EventHandler PrefixChanged;        

        private string _inputPrefix;        
        private int _numPosToMoveWhenOutOfScreen;        

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
                PrefixChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        public Color InputPrefixColor { get; set; }

        public int Length => MultiLineInput.ActiveLine.Buffer.Length;

        public int NumPositionsToMoveWhenOutOfScreen
        {
            get { return _numPosToMoveWhenOutOfScreen; }
            set { _numPosToMoveWhenOutOfScreen = Math.Max(value, 1); }
        }

        public Vector2 InputPrefixSize { get; set; }

        public void LoadContent(Console console)
        {
            Console = console;

            Console.FontChanged += (s, e) => CalculateInputPrefixWidth();            
            CalculateInputPrefixWidth();

            Caret.LoadContent(this);
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
            MultiLineInput.ActiveLine.Buffer.Insert(Caret.Index, value);
            Caret.MoveBy(value.Length);
        }
        
        public void Remove(int startIndex, int length)
        {
            Caret.Index = startIndex;
            MultiLineInput.ActiveLine.Buffer.Remove(startIndex, length);
        }
        
        public string Value
        {
            get { return MultiLineInput.ActiveLine.Buffer.ToString(); } // Does not allocate if value is cached.
            set
            {
                Clear();
                if (value != null)
                    MultiLineInput.ActiveLine.Buffer.Append(value);
                Caret.Index = MultiLineInput.ActiveLine.Buffer.Length;                
            }
        }

        public Vector2 MeasureSubstring(int startIndex, int length) => MultiLineInput.ActiveLine.Buffer.MeasureSubstring(startIndex, length);

        public string Substring(int startIndex, int length)
        {            
            return MultiLineInput.ActiveLine.Buffer.Substring(startIndex, length);
        }

        public string Substring(int startIndex)
        {            
            return MultiLineInput.ActiveLine.Buffer.Substring(startIndex);
        }

        public void Clear()
        {
            MultiLineInput.ActiveLine.Clear(); // TODO: clear all.
            Caret.MoveBy(int.MinValue);
            Cleared?.Invoke(this, EventArgs.Empty);
        }        

        public char this[int i]
        {
            get { return MultiLineInput.ActiveLine.Buffer[i]; }
            set { MultiLineInput.ActiveLine.Buffer[i] = value; }
        }                

        public void Update(float deltaSeconds)
        {
#if MONOGAME
            Input.Update();
#endif
            ProcessInput(Input);

            Caret.Update(deltaSeconds);
            RepeatingInput.Update(deltaSeconds);
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
            Selection.Draw();
            MultiLineInput.Draw();
            Caret.Draw();            
        }

        public void DrawStringOnActiveRow(string value, int startIndex)
        {
            float rowOffset = MultiLineInput.InputLines.Count - MultiLineInput.ActiveLineIndex;

            Vector2 position = new Vector2(
                Console.Padding + InputPrefixSize.X + MultiLineInput.ActiveLine.Buffer.MeasureSubstring(
                    MultiLineInput.ActiveLine.VisibleStartIndex, startIndex - MultiLineInput.ActiveLine.VisibleStartIndex).X,
                Console.WindowArea.Y + Console.WindowArea.Height - Console.Padding - Console.FontSize.Y * rowOffset);
            Console.SpriteBatch.DrawString(Console.Font, value, position, Console.FontColor);
        }
        
        public void SetDefaults(ConsoleSettings settings)
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
    }
}
