using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using Varus.Paradox.Console.Utilities;

namespace Varus.Paradox.Console
{
    /// <summary>
    /// A game system which enables an in-game window for typing commands.
    /// </summary>
    public partial class ConsoleShell : GameSystem
    {
        // General.
        private readonly ICommandInterpreter _commandInterpreter;
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private readonly Timer _transitionTimer = new Timer { AutoReset = false };
        
        private Texture2D _backgroundTexture;
        private SpriteFont _font;
        private RectangleF _windowArea;                        
        private float? _initialPadding;
        private float _padding;
        private float _heightRatio;
        private ConsoleState _state = ConsoleState.Closed;
        private bool _initialized;

        // Input history.        
        private readonly Stack<string> _inputHistoryBackward = new Stack<string>();
        private readonly Stack<string> _inputHistoryForward = new Stack<string>();

        private string _lastHistoryString;

        // User input.        
        private readonly Timer _repeatedPressTresholdTimer = new Timer { AutoReset = false };
        private readonly Timer _repeatedPressIntervalTimer = new Timer { AutoReset = true };

        private bool _startRepeatedProcess;
        private bool _isFastRepeating;
        private Keys _downKey;
        
        internal event EventHandler FontChanged = delegate { };
        internal event EventHandler PaddingChanged = delegate { };
        internal event EventHandler WindowAreaChanged = delegate { };        

        /// <summary>
        /// Initializes a new instance of <see cref="ConsoleShell"/>.
        /// </summary>
        /// <param name="registry">The registry.</param>        
        /// <param name="font">Font used in the <see cref="ConsoleShell"/> window.</param>
        /// <param name="commandInterpreter">
        /// User input interpreter. Manages autocompletion and the logic behind command execution.
        /// Pass NULL to use a stub command interpreter (useful for testing out shell itself).
        /// </param>
        public ConsoleShell(IServiceRegistry registry, SpriteFont font, ICommandInterpreter commandInterpreter)
            : base(registry)
        {
            Check.ArgumentNotNull(registry, "registry", "Cannot instantiate the shell without services container.");
            Check.ArgumentNotNull(font, "font", "Cannot instantiate the shell without a font.");
            
            CharWidthMap = new Dictionary<char, float>();            
            _commandInterpreter = commandInterpreter ?? new StubCommandInterpreter();
            _graphicsDeviceManager = (GraphicsDeviceManager)registry.GetSafeServiceAs<IGraphicsDeviceManager>();                
            Font = font;            
        }        

        /// <summary>
        /// Gets the input part of the <see cref="ConsoleShell"/>.
        /// </summary>
        public InputBuffer InputBuffer { get; private set; }

        /// <summary>
        /// Gets the output part of the <see cref="ConsoleShell"/>.
        /// </summary>
        public OutputBuffer OutputBuffer { get; private set; }

        /// <summary>
        /// Gets if any part of the <see cref="ConsoleShell"/> is visible.
        /// </summary>
        public bool IsVisible
        {
            get { return _state != ConsoleState.Closed; }
        }

        /// <summary>
        /// Gets if the <see cref="ConsoleShell"/> is currently accepting user input.
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
                Check.ArgumentNotNull(value, "value", "Font cannot be null.");
                _font = value;                
                CharWidthMap.Clear();                
                FontChanged(this, EventArgs.Empty);               
            }
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
        /// Gets or sets the time in seconds it takes to fully open or close the <see cref="ConsoleShell"/>.
        /// </summary>
        public float OpenCloseTransitionSeconds
        {
            get { return _transitionTimer.TargetTime; }
            set { _transitionTimer.TargetTime = value; }
        }

        /// <summary>
        /// Gets or sets the percentage of height the <see cref="ConsoleShell"/> window takes in relation to
        /// application window height. Value in between [0...1].
        /// </summary>
        public float HeightRatio
        {
            get { return _heightRatio; }
            set
            {
                _heightRatio = MathUtil.Clamp(value, 0, 1.0f);
                SetWindowWidthAndHeight(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);
            }
        }

        /// <summary>
        /// Gets or sets the padding to apply to the borders of the <see cref="ConsoleShell"/> window.
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
        /// shown in the <see cref="ConsoleShell"/>.
        /// </summary>
        public Dictionary<Keys, SymbolPair> SymbolMappings
        {
            get { return _symbolDefinitions; }
            set
            {
                Check.ArgumentNotNull(value, "value", "Symbol mappings cannot be null.");
                _symbolDefinitions = value;
            }
        } 

        internal Dictionary<char, float> CharWidthMap { get; private set; }

        internal SpriteBatch SpriteBatch { get; private set; }

        internal RectangleF WindowArea
        {
            get { return _windowArea; }
            set
            {
                value.Width = Math.Max(value.Width, 0);
                value.Height = Math.Max(value.Height, 0);
                _windowArea = value;
                WindowAreaChanged(this, EventArgs.Empty);
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
        /// Clears the subparts of the <see cref="ConsoleShell"/>.
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
        /// Clears the <see cref="ConsoleShell"/> and sets all the settings
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

        protected override void LoadContent()
        {
            _graphicsDeviceManager.PreparingDeviceSettings += OnPreparingDeviceChanged;
            SetWindowWidthAndHeight(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);            

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

        protected override void UnloadContent()
        {
            _graphicsDeviceManager.PreparingDeviceSettings -= OnPreparingDeviceChanged;

            SpriteBatch.Dispose();
            _backgroundTexture.Dispose();
        }


        #region Input handling
        
        private void HandleInput()
        {            
            if (!Input.IsKeyDown(_downKey))
            {
                ResetRepeatingPresses();
            }

            foreach (KeyEvent keyEvent in Input.KeyEvents)
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
                    string cmd = InputBuffer.Value;                    
                    // Determine if this is a line break or we should execute command straight away.
                    if (_actionDefinitions.BackwardTryGetValue(ConsoleAction.NextLineModifier, out modifier) &&
                        Input.IsKeyDown(modifier))
                    {                        
                        OutputBuffer.AddCommandEntry(cmd);
                    } 
                    else
                    {
                        if (OutputBuffer.HasCommandEntry)
                        {
                            cmd = OutputBuffer.DequeueCommandEntry() + cmd;                            
                        }

                        string[] cmdSplit = cmd.Split(NewLine, StringSplitOptions.None);
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
                    InputBuffer.Caret.MoveBy(int.MinValue);                    
                    return ConsoleProcessResult.Break;
                case ConsoleAction.PreviousCommandInHistory:
                    ManageHistory(_inputHistoryForward, _inputHistoryBackward);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.NextCommandInHistory:
                    ManageHistory(_inputHistoryBackward, _inputHistoryForward);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.Autocomplete:
                    bool hasModifier = _actionDefinitions.BackwardTryGetValue(ConsoleAction.AutocompleteModifier, out modifier);
                    if (hasModifier && !Input.IsKeyDown(modifier)) return ConsoleProcessResult.None;
                    bool canMoveBackwards = _actionDefinitions.BackwardTryGetValue(ConsoleAction.PreviousEntryModifier, out modifier);
                    _commandInterpreter.Autocomplete(InputBuffer, !canMoveBackwards || !Input.IsKeyDown(modifier));
                    InputBuffer.Caret.Index = InputBuffer.Length;
                    return ConsoleProcessResult.Break;
                case ConsoleAction.MoveLeft:
                    InputBuffer.Caret.MoveBy(-1);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.MoveRight:
                    InputBuffer.Caret.MoveBy(1);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.MoveToBeginning:
                    InputBuffer.Caret.Index = 0;
                    return ConsoleProcessResult.Break;
                case ConsoleAction.MoveToEnd:
                    InputBuffer.Caret.Index = InputBuffer.Length;
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
                        InputBuffer.Remove(InputBuffer.Caret.Index, 1);                        
                    }
                    return ConsoleProcessResult.Break;
                case ConsoleAction.Paste:                    
                    _actionDefinitions.BackwardTryGetValue(ConsoleAction.CopyPasteModifier, out modifier);
                    if (!Input.IsKeyDown(modifier)) break;
                    // TODO: Enable clipboard pasting. How to approach this in a cross-platform manner?
                    //string clipboardVal = Clipboard.GetText(TextDataFormat.Text);                        
                    //_currentInput.Append(clipboardVal);
                    //MoveCaret(clipboardVal.Length);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.Tab:
                    _actionDefinitions.BackwardTryGetValue(ConsoleAction.TabModifier, out modifier);
                    if (Input.IsKeyDown(modifier))
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

            bool toUpper = uppercaseModifiers != null && uppercaseModifiers.Any(x => Input.IsKeyDown(x));

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
        

        #region Input History

        private void ManageHistory(Stack<string> to, Stack<string> from)
        {
            // Check if there are any entries in the history.
            if (from.Count <= 0) return;

            // Add current to reverse history if it is not whitespace.
            if (!InputBuffer.IsEmptyOrWhitespace())
            {
                to.Push(InputBuffer.Value);
            }

            _lastHistoryString = from.Pop();
            InputBuffer.LastAutocompleteEntry = null;
            InputBuffer.Value = _lastHistoryString;
        }

        private void ClearHistory()
        {
            _lastHistoryString = null;
            _inputHistoryBackward.Clear();
            _inputHistoryForward.Clear();
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

        private void OnPreparingDeviceChanged(object sender, PreparingDeviceSettingsEventArgs args)
        {
            SetWindowWidthAndHeight(
                args.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth,
                args.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight);
        }

        private void SetWindowWidthAndHeight(int width, int height)
        {
            if (GraphicsDevice == null) return;

            var newWindowArea = new RectangleF(_windowArea.X, _windowArea.Y, 0, 0)
            {
                Width = width,
                Height = height * HeightRatio
            };
            switch (_state)
            {
                case ConsoleState.Closed:
                    newWindowArea.Y = -newWindowArea.Height;
                    break;
            }
            WindowArea = newWindowArea;
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
    }
}