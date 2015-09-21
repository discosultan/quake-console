using System;
using System.Collections.Generic;
using System.Linq;
using QuakeConsole.Utilities;
#if MONOGAME
using Texture = Microsoft.Xna.Framework.Graphics.Texture2D;
using MathUtil = Microsoft.Xna.Framework.MathHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endif

namespace QuakeConsole
{
    /// <summary>
    /// A game system which enables an in-game window for typing commands.
    /// </summary>
    internal partial class Console
    {
        internal event EventHandler FontChanged;
        internal event EventHandler PaddingChanged;
        internal event EventHandler WindowAreaChanged;

        // General.
        private ICommandInterpreter _commandInterpreter;
        private GraphicsDevice _device;
        private GraphicsDeviceManager _graphicsDeviceManager;

        private readonly Timer _transitionTimer = new Timer { AutoReset = false };        

        private Texture _backgroundTexture;
        private SpriteFont _font;
        private RectangleF _windowArea;                        
        private float? _initialPadding;
        private float _padding;
        private float _heightRatio;
        private ConsoleState _state = ConsoleState.Closed;
        private bool _loaded;

        // Input history.        
        private readonly List<string> _inputHistory = new List<string>();
        private int _inputHistoryIndexer;
        private bool _inputHistoryDoNotDecrement;

        // User input.        
        private readonly Timer _repeatedPressTresholdTimer = new Timer { AutoReset = false };
        private readonly Timer _repeatedPressIntervalTimer = new Timer { AutoReset = true };

        private bool _startRepeatedProcess;
        private bool _isFastRepeating;
        private Keys _downKey;                

        public Console()
        {            
            SetDefaults(_defaultSettings);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Console"/>.
        /// </summary>
        /// <param name="device">Graphcis device.</param>
        /// <param name="deviceManager">Graphics device manager.</param>        
        /// <param name="font">Font used in the <see cref="Console"/> window.</param>
        /// <param name="commandInterpreter">
        /// User input interpreter. Manages autocompletion and the logic behind command execution.
        /// Pass NULL to use a stub command interpreter (useful for testing out shell itself).
        /// </param>
        public void LoadContent(GraphicsDevice device, GraphicsDeviceManager deviceManager, SpriteFont font, ICommandInterpreter commandInterpreter)
        {
            Check.ArgumentNotNull(deviceManager, nameof(deviceManager), "Cannot instantiate the console without graphics device manager.");
            Check.ArgumentNotNull(font, nameof(font), "Cannot instantiate the console without a font.");

            _commandInterpreter = commandInterpreter ?? new StubCommandInterpreter();
            _device = device;
            _graphicsDeviceManager = deviceManager;
            Font = font;

            SpriteBatch = new SpriteBatch(_device);

            _graphicsDeviceManager.PreparingDeviceSettings += OnPreparingDeviceChanged;
#if MONOGAME
            _backgroundTexture = new Texture2D(_device, 2, 2, false, SurfaceFormat.Color);
            _backgroundTexture.SetData(new[] { Color.White, Color.White, Color.White, Color.White });
#else                   
            _backgroundTexture = Texture.New2D(GraphicsDevice, 2, 2, PixelFormat.R8G8B8A8_UNorm, new[] { Color.White, Color.White, Color.White, Color.White });            
#endif
            SetWindowWidthAndHeight();
            if (_initialPadding.HasValue)
                Padding = _initialPadding.Value;

            InputBuffer.LoadContent(this);
            OutputBuffer.LoadContent(this);

            _loaded = true;
        }

        public void UnloadContent()
        {
            _graphicsDeviceManager.PreparingDeviceSettings -= OnPreparingDeviceChanged;

            SpriteBatch.Dispose();
            _backgroundTexture.Dispose();
        }

        /// <summary>
        /// Gets the input part of the <see cref="Console"/>.
        /// </summary>
        public InputBuffer InputBuffer { get; } = new InputBuffer();

        /// <summary>
        /// Gets the output part of the <see cref="Console"/>.
        /// </summary>
        public OutputBuffer OutputBuffer { get; } = new OutputBuffer();

        /// <summary>
        /// Gets if any part of the <see cref="Console"/> is visible.
        /// </summary>
        public bool IsVisible => _state != ConsoleState.Closed;

        /// <summary>
        /// Gets if the <see cref="Console"/> is currently accepting user input.
        /// </summary>
        public bool IsAcceptingInput => _state == ConsoleState.Open || _state == ConsoleState.Opening;

        /// <summary>
        /// Gets or sets the input command logging delegate. Set this property to log the user input
        /// commands to the given delegate. For example WriteLine(String).
        /// </summary>
        public Action<string> LogInput { get; set; }

        /// <summary>
        /// Gets or sets the font.
        /// </summary>
        public SpriteFont Font
        {
            get { return _font; }
            set
            {
                Check.ArgumentNotNull(value, nameof(value), "Font cannot be null.");
                _font = value;                
                CharWidthMap.Clear();                
                FontChanged?.Invoke(this, EventArgs.Empty);               
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
                if (_loaded)
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
                PaddingChanged?.Invoke(this, EventArgs.Empty);
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
                Check.ArgumentNotNull(value, "value", "Symbol mappings cannot be null.");
                _symbolDefinitions = value;
            }
        }

#if MONOGAME
        internal InputManager Input { get; } = new InputManager();
#endif

        internal Dictionary<char, float> CharWidthMap { get; } = new Dictionary<char, float>();

        internal SpriteBatch SpriteBatch { get; private set; }

        internal RectangleF WindowArea
        {
            get { return _windowArea; }
            set
            {
                value.Width = Math.Max(value.Width, 0);
                value.Height = Math.Max(value.Height, 0);
                _windowArea = value;
                WindowAreaChanged?.Invoke(this, EventArgs.Empty);
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
        /// Clears the subparts of the <see cref="Console"/>.
        /// </summary>
        /// <param name="clearFlags">Specifies which subparts to clear.</param>
        public void Clear(ConsoleClearFlags clearFlags = ConsoleClearFlags.All)
        {
            if ((clearFlags & ConsoleClearFlags.OutputBuffer) != 0)
                OutputBuffer.Clear();
            if ((clearFlags & ConsoleClearFlags.InputBuffer) != 0)
                InputBuffer.Clear();
            if ((clearFlags & ConsoleClearFlags.InputHistory) != 0)
                ClearHistory();
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
        public void Update(float deltaSeconds)
        {
#if MONOGAME
            Input.Update();
#endif
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
        public void Draw()
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

#region Input handling
        
        private void HandleInput()
        {            
            if (!Input.IsKeyDown(_downKey))
                ResetRepeatingPresses();

            foreach (KeyEvent keyEvent in Input.KeyEvents)
            {
                // We are only interested in key presses.
                if (keyEvent.Type == KeyEventType.Released)
                    continue;

                if (keyEvent.Key != _downKey)
                {
                    ResetRepeatingPresses();
                    _downKey = keyEvent.Key;
                    _startRepeatedProcess = true;
                }

                ConsoleProcessResult result = HandleKey(keyEvent.Key);
                if (result == ConsoleProcessResult.Break)
                    break;
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
                        string executedCmd = cmd;
                        if (OutputBuffer.HasCommandEntry)
                        {
                            executedCmd = OutputBuffer.DequeueCommandEntry() + cmd;
                        }
                                                
                        // Replace our tab symbols with actual tab characters.
                        executedCmd = executedCmd.Replace(Tab, "\t");
                        // Log the command to be executed if logger is set.
                        LogInput?.Invoke(executedCmd);
                        // Execute command.
                        _commandInterpreter.Execute(OutputBuffer, executedCmd);
                    }

                    // Find the last historical entry if any.
                    string lastHistoricalEntry = null;
                    if (_inputHistory.Count > 0)
                        lastHistoricalEntry = _inputHistory[_inputHistory.Count - 1];

                    // Only add current command to input history if it is not an empty string and
                    // does not match the last historical entry.
                    if (cmd != "" && !cmd.Equals(lastHistoricalEntry, StringComparison.Ordinal))
                        _inputHistory.Add(cmd);
                    
                    InputBuffer.LastAutocompleteEntry = null;
                    // If the cmd matches the currently indexed historical entry then set a special flag
                    // which when moving backward in history, does not actually move backward, but will instead
                    // return the same entry that was returned before. This is similar to how Powershell and Cmd Prompt work.
                    if (_inputHistory.Count == 0 || _inputHistoryIndexer == int.MaxValue || !_inputHistory[_inputHistoryIndexer].Equals(cmd))
                        _inputHistoryIndexer = int.MaxValue;
                    else
                        _inputHistoryDoNotDecrement = true;

                    InputBuffer.Clear();                                        
                    InputBuffer.Caret.MoveBy(int.MinValue);                    
                    return ConsoleProcessResult.Break;
                case ConsoleAction.PreviousCommandInHistory:  
                    if (!_inputHistoryDoNotDecrement)
                        _inputHistoryIndexer--;                    
                    ManageHistory();                    
                    return ConsoleProcessResult.Break;                
                case ConsoleAction.NextCommandInHistory:                
                    _inputHistoryIndexer++;                    
                    ManageHistory();                    
                    return ConsoleProcessResult.Break;                
                case ConsoleAction.Autocomplete:
                    bool hasModifier = _actionDefinitions.BackwardTryGetValue(ConsoleAction.AutocompleteModifier, out modifier);
                    if (hasModifier && !Input.IsKeyDown(modifier)) return ConsoleProcessResult.None;
                    bool canMoveBackwards = _actionDefinitions.BackwardTryGetValue(ConsoleAction.PreviousEntryModifier, out modifier);
                    _commandInterpreter.Autocomplete(InputBuffer, !canMoveBackwards || !Input.IsKeyDown(modifier));
                    InputBuffer.Caret.Index = InputBuffer.Length;
                    _inputHistoryIndexer = int.MaxValue;
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
                        ResetLastHistoryAndAutocompleteEntries();
                    }
                    return ConsoleProcessResult.Break;
                case ConsoleAction.DeleteCurrentChar:
                    if (InputBuffer.Length > InputBuffer.Caret.Index)
                    {                        
                        InputBuffer.Remove(InputBuffer.Caret.Index, 1);
                        ResetLastHistoryAndAutocompleteEntries();
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
            _inputHistoryIndexer = int.MaxValue;
        }

#endregion
        

#region Input History

        private void ManageHistory()
        {
            // Check if there are any entries in the history.
            if (_inputHistory.Count <= 0) return;

            _inputHistoryIndexer = MathUtil.Clamp(_inputHistoryIndexer, 0, _inputHistory.Count - 1);

            _inputHistoryDoNotDecrement = false;
            InputBuffer.LastAutocompleteEntry = null;
            InputBuffer.Value = _inputHistory[_inputHistoryIndexer];
        }

        private void ClearHistory()
        {
            _inputHistory.Clear();
            _inputHistoryIndexer = int.MaxValue;
            _inputHistoryDoNotDecrement = false;
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

        private void SetWindowWidthAndHeight()
        {
#if MONOGAME
            SetWindowWidthAndHeight(_device.Viewport.Width, _device.Viewport.Height);            
#else
            SetWindowWidthAndHeight(_device.BackBuffer.Width, _device.BackBuffer.Height);            
#endif
        }

        private void SetWindowWidthAndHeight(int width, int height)
        {
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
            FontColor = _defaultSettings.FontColor;
            HeightRatio = _defaultSettings.HeightRatio;
            OpenCloseTransitionSeconds = _defaultSettings.OpenCloseTransitionSeconds;
            RepeatingInputCooldown = _defaultSettings.RepeatingInputCooldown;
            TimeUntilRepeatingInput = _defaultSettings.TimeUntilRepeatingInput;                        
            Padding = settings.Padding;

            InputBuffer.SetDefaults(settings);
            OutputBuffer.SetDefaults(settings);
        }           
    }
}