using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using Varus.Paradox.Console.Utilities;

namespace Varus.Paradox.Console
{
    /// <summary>
    /// A game system which enables an in-game window for typing commands.
    /// </summary>
    public class Console : GameSystemBase
    {        
        #region Definitions
        
        private readonly BiDirectionalDictionary<Keys, ConsoleAction> _actionDefinitions = new BiDirectionalDictionary<Keys, ConsoleAction>
        {
            { Keys.Enter, ConsoleAction.ExecuteCommand },
            { Keys.NumPadEnter, ConsoleAction.ExecuteCommand },
            { Keys.Left, ConsoleAction.MoveLeft },
            { Keys.Right, ConsoleAction.MoveRight },
            { Keys.Home, ConsoleAction.MoveToBeginning },
            { Keys.End, ConsoleAction.MoveToEnd },
            { Keys.Back, ConsoleAction.DeletePreviousChar },
            { Keys.Delete, ConsoleAction.DeleteCurrentChar },
            { Keys.LeftShift, ConsoleAction.UppercaseModifier },
            { Keys.Up, ConsoleAction.PreviousCommandInHistory },
            { Keys.Down, ConsoleAction.NextCommandInHistory },            
            { Keys.LeftCtrl, ConsoleAction.CopyPasteModifier },
            { Keys.LeftShift, ConsoleAction.PreviousEntryModifier },
            { Keys.V, ConsoleAction.Paste },
            { Keys.LeftShift, ConsoleAction.NextLineModifier },
            //{ Keys.Tab, ConsoleAction.Autocomplete },            
            { Keys.Space, ConsoleAction.Autocomplete },
            { Keys.LeftCtrl, ConsoleAction.AutocompleteModifier },
            { Keys.Tab, ConsoleAction.Tab },
            { Keys.LeftShift, ConsoleAction.TabModifier }
        };

        private Dictionary<Keys, SymbolPair> _symbolDefinitions = new Dictionary<Keys, SymbolPair>
        {
            // Digits.
            { Keys.D1, new SymbolPair("1", "!") },
            { Keys.D2, new SymbolPair("2", "@") },
            { Keys.D3, new SymbolPair("3", "#") },
            { Keys.D4, new SymbolPair("4", "$") },
            { Keys.D5, new SymbolPair("5", "%") },
            { Keys.D6, new SymbolPair("6", "^") },
            { Keys.D7, new SymbolPair("7", "&") },
            { Keys.D8, new SymbolPair("8", "*") },
            { Keys.D9, new SymbolPair("9", "(") },
            { Keys.D0, new SymbolPair("0", ")") },
            { Keys.NumPad1, new SymbolPair("1") },
            { Keys.NumPad2, new SymbolPair("2") },
            { Keys.NumPad3, new SymbolPair("3") },
            { Keys.NumPad4, new SymbolPair("4") },
            { Keys.NumPad5, new SymbolPair("5") },
            { Keys.NumPad6, new SymbolPair("6") },
            { Keys.NumPad7, new SymbolPair("7") },
            { Keys.NumPad8, new SymbolPair("8") },
            { Keys.NumPad9, new SymbolPair("9") },
            { Keys.NumPad0, new SymbolPair("0") },

            // Letters.
            { Keys.Q, new SymbolPair("q", "Q") },
            { Keys.W, new SymbolPair("w", "W") },
            { Keys.E, new SymbolPair("e", "E") },
            { Keys.R, new SymbolPair("r", "R") },
            { Keys.T, new SymbolPair("t", "T") },
            { Keys.Y, new SymbolPair("y", "Y") },
            { Keys.U, new SymbolPair("u", "U") },
            { Keys.I, new SymbolPair("i", "I") },
            { Keys.O, new SymbolPair("o", "O") },
            { Keys.P, new SymbolPair("p", "P") },
            { Keys.OemOpenBrackets, new SymbolPair("[", "{") },
            { Keys.OemCloseBrackets, new SymbolPair("]", "}") },

            { Keys.A, new SymbolPair("a", "A") },
            { Keys.S, new SymbolPair("s", "S") },
            { Keys.D, new SymbolPair("d", "D") },
            { Keys.F, new SymbolPair("f", "F") },
            { Keys.G, new SymbolPair("g", "G") },
            { Keys.H, new SymbolPair("h", "H") },
            { Keys.J, new SymbolPair("j", "J") },
            { Keys.K, new SymbolPair("k", "K") },
            { Keys.L, new SymbolPair("l", "L") },
            { Keys.OemSemicolon, new SymbolPair(";", ":") },
            { Keys.OemQuotes, new SymbolPair("'", "\"") },
            { Keys.Oem5, new SymbolPair("\\", "|") },            

            { Keys.OemBackslash, new SymbolPair("\\", "|") },
            { Keys.Z, new SymbolPair("z", "Z") },
            { Keys.X, new SymbolPair("x", "X") },
            { Keys.C, new SymbolPair("c", "C") },
            { Keys.V, new SymbolPair("v", "V") },
            { Keys.B, new SymbolPair("b", "B") },
            { Keys.N, new SymbolPair("n", "N") },
            { Keys.M, new SymbolPair("m", "M") },
            { Keys.OemComma, new SymbolPair(",", "<") },
            { Keys.OemPeriod, new SymbolPair(".", ">") },
            { Keys.Oem2, new SymbolPair("/", "?") },

            // Special.
            { Keys.Space, new SymbolPair(" ", " ") },
            { Keys.OemMinus, new SymbolPair("-", "_") },
            { Keys.OemPlus, new SymbolPair("=", "+") },
            { Keys.Decimal, new SymbolPair(".") },
            { Keys.Add, new SymbolPair("+") },
            { Keys.Subtract, new SymbolPair("-") },
            { Keys.Multiply, new SymbolPair("*") },
            { Keys.Divide, new SymbolPair("/") },
            //{ Keys.Tab, new SymbolPair("\t", "\t") } // Tab char is not supported in many fonts.
            //{ Keys.Tab, new SymbolPair("    ", "    ") } // Use 4 spaces instead.
        };

        internal readonly string NewLine = "\n";
        internal readonly string Tab = "    ";

        #endregion

        private readonly ICommandInterpreter _commandInterpreter;

        private readonly ConsoleSettings _defaultSettings = new ConsoleSettings
        {            
            BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f),
            FontColor = new Color(1.0f, 1.0f, 0.0f, 1.0f),
            OpenCloseTransitionSeconds = 0.25f,
            TimeUntilRepeatingInput = 0.4f,
            RepeatingInputCooldown = 0.04f,
            HeightRatio = 0.4f,
            Enabled = true,
            Visible = true,
            InputPrefix = "]",
            NumPositionsToMoveWhenOutOfScreen = 4,
            InputPrefixColor = Color.Yellow,
            Padding = 2,
            CaretSymbol = "_",
            CaretBlinkingIntervalSeconds = 0.4f
        };

        private bool _initialized;        
        private Texture2D _backgroundTexture;
        private readonly InputManager _inputManager;
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private SpriteFont _font;
        private float _padding;
        private float? _initialPadding;
        private RectangleF _windowArea;        
        private readonly Timer _transitionTimer = new Timer { AutoReset = false };        
        private ConsoleState _state = ConsoleState.Closed;        
        private float _heightRatio;

        internal Dictionary<char, float> CharWidthMap { get; private set; }

        // History.
        private string _lastHistoryString;
        private readonly Stack<string> _inputHistoryBackward = new Stack<string>();
        private readonly Stack<string> _inputHistoryForward = new Stack<string>();

        // Input.        
        private readonly Timer _repeatedPressTresholdTimer = new Timer { AutoReset = false };
        private readonly Timer _repeatedPressIntervalTimer = new Timer { AutoReset = true };
        private bool _startRepeatedProcess;
        private bool _isFastRepeating;
        private Keys _downKey;
        
        internal event EventHandler FontChanged = delegate { };
        internal event EventHandler PaddingChanged = delegate { };
        internal event EventHandler WindowAreaChanged = delegate { };        

        /// <summary>
        /// Initializes a new instance of <see cref="Console"/>.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <param name="commandInterpreter">User input interpreter. Manages autocompletion and command execution.</param>
        /// <param name="font">Font used in the <see cref="Console"/> window.</param>
        public Console(IServiceRegistry registry, ICommandInterpreter commandInterpreter, SpriteFont font)
            : base(registry)
        {
            Check.ArgumentNotNull(registry, "registry");
            Check.ArgumentNotNull(commandInterpreter, "commandInterpreter");
            Check.ArgumentNotNull(font, "font");            
            
            CharWidthMap = new Dictionary<char, float>();            
            _commandInterpreter = commandInterpreter;
            _graphicsDeviceManager = (GraphicsDeviceManager)registry.GetSafeServiceAs<IGraphicsDeviceManager>();
            _inputManager = registry.GetSafeServiceAs<InputManager>();            
            Font = font;            
        }

        internal SpriteBatch SpriteBatch { get; private set; }

        internal RectangleF WindowArea
        {
            get { return _windowArea; }
            set
            {
                Check.ArgumentNotLessThan(value.Width, 0, "value");
                Check.ArgumentNotLessThan(value.Height, 0, "value");
                _windowArea = value; 
                WindowAreaChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the input part of the <see cref="Console"/>.
        /// </summary>
        public InputBuffer InputBuffer { get; private set; }

        /// <summary>
        /// Gets the output part of the <see cref="Console"/>.
        /// </summary>
        public OutputBuffer OutputBuffer { get; private set; }

        /// <summary>
        /// Gets if any part of the <see cref="Console"/> is visible.
        /// </summary>
        public bool IsVisible
        {
            get { return _state != ConsoleState.Closed; }
        }

        /// <summary>
        /// Gets if the <see cref="Console"/> is currently accepting user input.
        /// </summary>
        public bool IsAcceptingInput
        {
            get { return _state == ConsoleState.Open || _state == ConsoleState.Opening; }
        }

        /// <summary>
        /// Gets or sets the font.
        /// </summary>
        public SpriteFont Font
        {
            get { return _font; }
            set
            {
                Check.ArgumentNotNull(value, "value");
                _font = value;                
                CharWidthMap.Clear();                
                FontChanged(this, EventArgs.Empty);               
            }
        }

        internal float RepeatingInputCooldown
        {
            get { return _repeatedPressIntervalTimer.TargetTime; }
            set { _repeatedPressIntervalTimer.TargetTime = value; }
        }

        internal float TimeUntilRepeatingInput
        {
            get { return _repeatedPressTresholdTimer.TargetTime; }
            set { _repeatedPressTresholdTimer.TargetTime = value; }
        }

        /// <summary>
        /// Gets or sets the background color. Supports transparency.
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the font color. Supports transparency.
        /// </summary>
        public Color FontColor { get; set; }

        /// <summary>
        /// Gets or sets the time in seconds it takes to fully open or close the <see cref="Console"/>.
        /// </summary>
        public float OpenCloseTransitionSeconds
        {
            get { return _transitionTimer.TargetTime; }
            set { _transitionTimer.TargetTime = value; }
        }

        /// <summary>
        /// Gets or sets the percentage of height the <see cref="Console"/> window takes in relation to
        /// application window height. Value in between [0...1].
        /// </summary>
        public float HeightRatio
        {
            get { return _heightRatio; }
            set
            {
                _heightRatio = MathUtil.Clamp(value, 0, 1.0f);
                SetWindowWidthAndHeight();                
            }
        }

        /// <summary>
        /// Gets or sets the padding to apply to the borders of the <see cref="Console"/> window.
        /// Note that padding will be automatically decreased if the available window area becomes too low.
        /// </summary>
        public float Padding
        {
            get { return _padding; }
            set
            {
                // Store the padding anyway. The console might not be fully loaded before the user
                // can already set the padding. We can set it after loading once _initialPadding has been set.
                _initialPadding = value;
                _padding = MathUtil.Clamp(
                    value, 
                    0,
                    GetMaxAllowedPadding());
                PaddingChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the dictionary that is used to map keyboard keys to corresponding symbols
        /// shown in the <see cref="Console"/>.
        /// </summary>
        public Dictionary<Keys, SymbolPair> SymbolMappings
        {
            get { return _symbolDefinitions; }
            set
            {
                Check.ArgumentNotNull(value, "value");
                _symbolDefinitions = value;
            }
        }        

        protected override void LoadContent()
        {
            _graphicsDeviceManager.PreparingDeviceSettings += SetWindowWidthAndHeight;
            SetWindowWidthAndHeight();            

            SpriteBatch = new SpriteBatch(GraphicsDevice);
            _backgroundTexture = Texture2D.New(GraphicsDevice, 2, 2, PixelFormat.R8G8B8A8_UNorm, new[] { Color.White, Color.White, Color.White, Color.White });            
            OutputBuffer = new OutputBuffer(this);
            InputBuffer = new InputBuffer(this);

            SetDefaults(_defaultSettings);

            if (_initialized) return;

            _initialized = true;
            if (_initialPadding.HasValue)
                Padding = _initialPadding.Value;
        }

        /// <summary>
        /// Opens the console windows if it is closed. Closes it if it is opened.
        /// </summary>
        public void ToggleOpenClose()
        {
            switch (_state)
            {
                case ConsoleState.Closed:
                    _state = ConsoleState.Opening;
                    _transitionTimer.Reset();
                    break;
                case ConsoleState.Open:
                    _state = ConsoleState.Closing;
                    _transitionTimer.Reset();
                    break;
            }
        }

        /// <summary>
        /// Clears the subparts of the <see cref="Console"/>.
        /// </summary>
        /// <param name="clearFlags">Specifies which subparts to clear.</param>
        public void Clear(ConsoleClearFlags clearFlags = ConsoleClearFlags.All)
        {
            if ((clearFlags & ConsoleClearFlags.OutputBuffer) != 0)
            {
                OutputBuffer.Clear();
            }
            if ((clearFlags & ConsoleClearFlags.InputBuffer) != 0)
            {
                InputBuffer.Clear();                
            }
            if ((clearFlags & ConsoleClearFlags.InputHistory) != 0)
            {
                ClearHistory();
            }       
        }

        /// <summary>
        /// Clears the <see cref="Console"/> and sets all the settings
        /// to their default values.
        /// </summary>
        public void Reset()
        {
            Clear();
            SetDefaults(_defaultSettings);
        }

        /// <inheritdoc/>        
        public override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.Elapsed.TotalSeconds; 
            switch (_state)
            {
                case ConsoleState.Closing:
                    UpdateTimer(deltaSeconds, ConsoleState.Closed);
                    WindowArea = new RectangleF(
                        WindowArea.X, 
                        -WindowArea.Height * _transitionTimer.Progress, 
                        WindowArea.Width, 
                        WindowArea.Height);
                    InputBuffer.Update(deltaSeconds);
                    break;
                case ConsoleState.Opening:
                    UpdateTimer(deltaSeconds, ConsoleState.Open);
                    WindowArea = new RectangleF(
                        WindowArea.X,
                        -WindowArea.Height + WindowArea.Height * _transitionTimer.Progress, 
                        WindowArea.Width,
                        WindowArea.Height);
                    goto case ConsoleState.Open;
                case ConsoleState.Open:                    
                    HandleInput();
                    if (_startRepeatedProcess && !_isFastRepeating)
                    {
                        _repeatedPressTresholdTimer.Update(deltaSeconds);
                        if (_repeatedPressTresholdTimer.Finished)
                        {
                            _isFastRepeating = true;
                            _repeatedPressIntervalTimer.Reset();
                        }
                    }
                    else if (_isFastRepeating)
                    {
                        _repeatedPressIntervalTimer.Update(deltaSeconds);
                        if (_repeatedPressIntervalTimer.Finished)
                        {
                            HandleKey(_downKey);                            
                        }
                    }
                    InputBuffer.Update(deltaSeconds);
                    break;
            }
        }

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime)
        {
            switch (_state)
            {
                case ConsoleState.Closing:
                case ConsoleState.Opening:
                case ConsoleState.Open:
                    SpriteBatch.Begin();
                    // Draw background.
                    SpriteBatch.Draw(
                        _backgroundTexture,
                        WindowArea,
                        BackgroundColor);
                    // Draw output buffer.                    
                    OutputBuffer.Draw();
                    // Draw input buffer.
                    InputBuffer.Draw();                    
                    SpriteBatch.End();
                    break;
            }
        }

        protected override void UnloadContent()
        {
            _graphicsDeviceManager.PreparingDeviceSettings -= SetWindowWidthAndHeight;

            SpriteBatch.Dispose();
            _backgroundTexture.Dispose();
        }


        #region Input handling
        
        private void HandleInput()
        {            
            if (!_inputManager.IsKeyDown(_downKey))
            {
                ResetRepeatingPresses();
            }

            foreach (KeyEvent keyEvent in _inputManager.KeyEvents)
            {
                // We are only interested in key presses.
                if (keyEvent.Type == KeyEventType.Released) continue;

                if (keyEvent.Key != _downKey)
                {
                    ResetRepeatingPresses();
                    _downKey = keyEvent.Key;
                    _startRepeatedProcess = true;
                }

                ConsoleProcessResult result = HandleKey(keyEvent.Key);
                if (result == ConsoleProcessResult.Break) break;
            }
        }

        private ConsoleProcessResult HandleKey(Keys key)
        {
            ConsoleProcessResult processResult = ProcessSpecialKey(key);
            if (processResult == ConsoleProcessResult.Continue ||
                processResult == ConsoleProcessResult.Break)
            {
                return processResult;
            }
            processResult = ProcessRegularKey(key);
            return processResult;
        }

        private ConsoleProcessResult ProcessSpecialKey(Keys key)
        {
            ConsoleAction action;
            if (!_actionDefinitions.ForwardTryGetValue(key, out action))
                return ConsoleProcessResult.None;

            Keys modifier;
            switch (action)
            {                                    
                case ConsoleAction.ExecuteCommand:
                    string cmd = InputBuffer.Get();                    
                    // Determine if this is a line break or we should execute command straight away.
                    if (_actionDefinitions.BackwardTryGetValue(ConsoleAction.NextLineModifier, out modifier) &&
                        _inputManager.IsKeyDown(modifier))
                    {                        
                        OutputBuffer.AddCommandEntry(cmd);
                    } 
                    else
                    {
                        if (OutputBuffer.HasCommandEntry())
                        {
                            cmd = OutputBuffer.DequeueCommandEntry() + cmd;                            
                        }

                        string[] cmdSplit = cmd.Split(NewLine.AsArray(), StringSplitOptions.None);
                        foreach (string split in cmdSplit)
                        {
                            // Save command to history.
                            if (_inputHistoryBackward.Count == 0 ||
                                !_inputHistoryBackward.Peek().Equals(split, StringComparison.Ordinal))
                            {
                                _inputHistoryBackward.Push(split);
                            }
                        }
                        // Execute command.
                        _commandInterpreter.Execute(OutputBuffer, cmd.Replace(Tab, "\t"));
                    }
                    InputBuffer.Clear();
                    InputBuffer.LastAutocompleteEntry = null;
                    InputBuffer.Caret.Move(int.MinValue);                    
                    return ConsoleProcessResult.Break;
                case ConsoleAction.PreviousCommandInHistory:
                    ManageHistory(_inputHistoryForward, _inputHistoryBackward);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.NextCommandInHistory:
                    ManageHistory(_inputHistoryBackward, _inputHistoryForward);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.Autocomplete:
                    bool hasModifier = _actionDefinitions.BackwardTryGetValue(ConsoleAction.AutocompleteModifier, out modifier);
                    if (hasModifier && !_inputManager.IsKeyDown(modifier)) return ConsoleProcessResult.None;
                    bool canMoveBackwards = _actionDefinitions.BackwardTryGetValue(ConsoleAction.PreviousEntryModifier, out modifier);
                    _commandInterpreter.Autocomplete(InputBuffer, !canMoveBackwards || !_inputManager.IsKeyDown(modifier));
                    InputBuffer.Caret.MoveTo(InputBuffer.Length);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.MoveLeft:
                    InputBuffer.Caret.Move(-1);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.MoveRight:
                    InputBuffer.Caret.Move(1);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.MoveToBeginning:
                    InputBuffer.Caret.MoveTo(0);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.MoveToEnd:
                    InputBuffer.Caret.MoveTo(InputBuffer.Length);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.DeletePreviousChar:
                    if (InputBuffer.Length > 0 && InputBuffer.Caret.Index > 0)
                    {                        
                        InputBuffer.Remove(Math.Max(0, InputBuffer.Caret.Index - 1), 1);                        
                    }
                    return ConsoleProcessResult.Break;
                case ConsoleAction.DeleteCurrentChar:
                    if (InputBuffer.Length > InputBuffer.Caret.Index)
                    {
                        bool needToMoveCaret = InputBuffer.Caret.Index > 0;
                        InputBuffer.Remove(InputBuffer.Caret.Index, 1);
                        if (needToMoveCaret) InputBuffer.Caret.Move(1);
                    }
                    return ConsoleProcessResult.Break;
                case ConsoleAction.Paste:                    
                    _actionDefinitions.BackwardTryGetValue(ConsoleAction.CopyPasteModifier, out modifier);
                    if (!_inputManager.IsKeyDown(modifier)) break;
                    // TODO: Enable clipboard pasting. How to approach this in a cross-platform manner?
                    //string clipboardVal = Clipboard.GetText(TextDataFormat.Text);                        
                    //_currentInput.Append(clipboardVal);
                    //MoveCaret(clipboardVal.Length);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.Tab:
                    _actionDefinitions.BackwardTryGetValue(ConsoleAction.TabModifier, out modifier);
                    if (_inputManager.IsKeyDown(modifier))
                    {
                        InputBuffer.RemoveTab();
                    }
                    else
                    {
                        InputBuffer.Write(Tab);
                    }                                        
                    ResetLastHistoryAndAutocompleteEntries(); 
                    return ConsoleProcessResult.Break;
            }

            return ConsoleProcessResult.None;
        }

        private ConsoleProcessResult ProcessRegularKey(Keys key)
        {
            SymbolPair symbolPair;
            if (!_symbolDefinitions.TryGetValue(key, out symbolPair))
                return ConsoleProcessResult.None;

            List<Keys> uppercaseModifiers;
            _actionDefinitions.BackwardTryGetValues(ConsoleAction.UppercaseModifier, out uppercaseModifiers);

            bool toUpper = uppercaseModifiers != null && uppercaseModifiers.Any(x => _inputManager.IsKeyDown(x));

            InputBuffer.Write(toUpper
                ? symbolPair.UppercaseSymbol
                : symbolPair.LowercaseSymbol);
            
            ResetLastHistoryAndAutocompleteEntries(); 
            return ConsoleProcessResult.Break;
        }

        private void ResetLastHistoryAndAutocompleteEntries()
        {
            InputBuffer.LastAutocompleteEntry = null;
            if (_lastHistoryString != null &&
                (_inputHistoryBackward.Count == 0 ||
                 !_inputHistoryBackward.Peek().Equals(_lastHistoryString, StringComparison.OrdinalIgnoreCase)))
            {
                _inputHistoryBackward.Push(_lastHistoryString);
                _lastHistoryString = null;
            }
        }

        #endregion                
        

        private void UpdateTimer(float deltaSeconds, ConsoleState stateToSetWhenFinished)
        {
            _transitionTimer.Update(deltaSeconds);
            if (_transitionTimer.Finished)
            {
                _state = stateToSetWhenFinished;
            }
        }

        private void SetWindowWidthAndHeight()
        {
            if (GraphicsDevice == null) return;
            SetWindowWidthAndHeight(null, new PreparingDeviceSettingsEventArgs(new GraphicsDeviceInformation
            {
                PresentationParameters =
                    new PresentationParameters
                    {
                        BackBufferWidth = GraphicsDevice.BackBuffer.Width,
                        BackBufferHeight = GraphicsDevice.BackBuffer.Height
                    }
            }));
        }

        private void SetWindowWidthAndHeight(object sender, PreparingDeviceSettingsEventArgs args)
        {
            var windowArea = new RectangleF(_windowArea.X, _windowArea.Y, 0, 0)
            {
                Width = args.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth,
                Height = args.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight * HeightRatio
            };
            switch (_state)
            {
                case ConsoleState.Closed:
                    windowArea.Y = -WindowArea.Height;
                    break;
            }
            WindowArea = windowArea;
            if (_padding > GetMaxAllowedPadding()) 
                Padding = _padding; // Invoke padding setter.
        }

        private float GetMaxAllowedPadding()
        {
            return Math.Min(_windowArea.Width / 2 - _padding / 2, _windowArea.Height / 2 - _padding / 2);
        }

        private void ResetRepeatingPresses()
        {
            _downKey = Keys.None;
            _startRepeatedProcess = false;
            _isFastRepeating = false;
            _repeatedPressTresholdTimer.Reset();
            _repeatedPressIntervalTimer.Reset();
        }


        #region Input history

        private void ManageHistory(Stack<string> to, Stack<string> from)
        {
            // Check if there are any entries in the history.
            if (from.Count <= 0) return;

            // Add current to reverse history if it is not whitespace.
            if (!InputBuffer.IsEmptyOrWhitespace())
            {
                to.Push(InputBuffer.Get());
            }

            _lastHistoryString = from.Pop();
            InputBuffer.LastAutocompleteEntry = null;
            InputBuffer.Set(_lastHistoryString);            
        }

        private void ClearHistory()
        {
            _lastHistoryString = null;
            _inputHistoryBackward.Clear();
            _inputHistoryForward.Clear();
        }

        private void SetDefaults(ConsoleSettings settings)
        {
            BackgroundColor = _defaultSettings.BackgroundColor;
            Enabled = _defaultSettings.Enabled;
            FontColor = _defaultSettings.FontColor;
            HeightRatio = _defaultSettings.HeightRatio;
            OpenCloseTransitionSeconds = _defaultSettings.OpenCloseTransitionSeconds;
            RepeatingInputCooldown = _defaultSettings.RepeatingInputCooldown;
            TimeUntilRepeatingInput = _defaultSettings.TimeUntilRepeatingInput;            
            Visible = _defaultSettings.Visible;
            Padding = settings.Padding;

            InputBuffer.SetDefaults(settings);
            OutputBuffer.SetDefaults(settings);
        }

        #endregion                
    }
}