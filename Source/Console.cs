using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
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
    internal partial class Console
    {
        internal event EventHandler FontChanged;
        internal event EventHandler PaddingChanged;
        internal event EventHandler WindowAreaChanged;

        // General.        
        private ICommandInterpreter _commandInterpreter;
        private GraphicsDeviceManager _graphicsDeviceManager;

        private readonly Timer _transitionTimer = new Timer { AutoReset = false };
        
        private Texture _whiteTexture;
        private SpriteFont _font;
        private RectangleF _windowArea;        
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
            SetDefaults(new ConsoleSettings());
        }

        public void LoadContent(GraphicsDevice device, GraphicsDeviceManager deviceManager, ContentManager content, 
            SpriteFont font, ICommandInterpreter commandInterpreter)
        {
            Check.ArgumentNotNull(deviceManager, nameof(deviceManager), "Cannot instantiate the console without graphics device manager.");
            Check.ArgumentNotNull(font, nameof(font), "Cannot instantiate the console without a font.");
            Check.ArgumentNotNull(content, nameof(content));

            _commandInterpreter = commandInterpreter ?? new StubCommandInterpreter();
            GraphicsDevice = device;
            Content = content;
            _graphicsDeviceManager = deviceManager;
            Font = font;            

            SpriteBatch = new SpriteBatch(GraphicsDevice);

            _graphicsDeviceManager.PreparingDeviceSettings += OnPreparingDeviceChanged;
#if MONOGAME
            _whiteTexture = new Texture2D(GraphicsDevice, 2, 2, false, SurfaceFormat.Color);
            _whiteTexture.SetData(new[] { Color.White, Color.White, Color.White, Color.White });
#else                   
            _backgroundTexture = Texture.New2D(GraphicsDevice, 2, 2, PixelFormat.R8G8B8A8_UNorm, new[] { Color.White, Color.White, Color.White, Color.White });            
#endif
            _loaded = true;

            SetWindowWidthAndHeight();            

            ConsoleInput.LoadContent(this);
            ConsoleOutput.LoadContent(this);
            BgRenderer.LoadContent(this);
        }

        public void UnloadContent()
        {
            _graphicsDeviceManager.PreparingDeviceSettings -= OnPreparingDeviceChanged;

            SpriteBatch.Dispose();
            _whiteTexture.Dispose();            
            BgRenderer.UnloadContent();
        }

        public ICommandInterpreter Interpreter
        {
            get { return _commandInterpreter; }
            set { _commandInterpreter = value ?? new StubCommandInterpreter(); }
        }

        public ConsoleInput ConsoleInput { get; } = new ConsoleInput();

        public ConsoleOutput ConsoleOutput { get; } = new ConsoleOutput();        
        
        public bool IsVisible => _state != ConsoleState.Closed;
        
        public bool IsAcceptingInput => _state == ConsoleState.Open || _state == ConsoleState.Opening;
        
        public Action<string> LogInput { get; set; }
        
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
        
        public Color BackgroundColor { get; set; }
        
        public Color FontColor { get; set; }        
        
        public float OpenCloseTransitionSeconds
        {
            get { return _transitionTimer.TargetTime; }
            set { _transitionTimer.TargetTime = value; }
        }
        
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
        
        public float Padding
        {
            get { return _padding; }
            set
            {                
                if (_loaded)
                {
                    _padding = MathUtil.Clamp(
                        value,
                        0,
                        GetMaxAllowedPadding());
                    PaddingChanged?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    _padding = value;
                }
            }
        }

        public Color BottomBorderColor { get; set; }
        public float BottomBorderThickness { get; set; }        
        
        public Dictionary<Keys, SymbolPair> SymbolMappings
        {
            get { return _symbolDefinitions; }
            set
            {
                Check.ArgumentNotNull(value, "value", "Symbol mappings cannot be null.");
                _symbolDefinitions = value;
            }
        }

        public float RepeatingInputCooldown
        {
            get { return _repeatedPressIntervalTimer.TargetTime; }
            set { _repeatedPressIntervalTimer.TargetTime = value; }
        }

        public float TimeUntilRepeatingInput
        {
            get { return _repeatedPressTresholdTimer.TargetTime; }
            set { _repeatedPressTresholdTimer.TargetTime = value; }
        }

#if MONOGAME
        internal InputManager Input { get; } = new InputManager();
#endif

        internal Dictionary<char, float> CharWidthMap { get; } = new Dictionary<char, float>();

        internal BackgroundRenderer BgRenderer { get; } = new BackgroundRenderer();
        internal SpriteBatch SpriteBatch { get; private set; }
        internal GraphicsDevice GraphicsDevice { get; private set; }
        internal ContentManager Content { get; private set; }

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
        
        public void Clear(ConsoleClearFlags clearFlags = ConsoleClearFlags.All)
        {
            if ((clearFlags & ConsoleClearFlags.OutputBuffer) != 0)
                ConsoleOutput.Clear();
            if ((clearFlags & ConsoleClearFlags.InputBuffer) != 0)
                ConsoleInput.Clear();
            if ((clearFlags & ConsoleClearFlags.InputHistory) != 0)
                ClearHistory();
        }
        
        public void Reset()
        {
            Clear();
            SetDefaults(new ConsoleSettings());
        }
        
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
                    ConsoleInput.Update(deltaSeconds);
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
                            HandleKey(_downKey);
                    }
                    ConsoleInput.Update(deltaSeconds);
                    break;
            }
        }

        public void Draw()
        {
            switch (_state)
            {
                case ConsoleState.Closing:
                case ConsoleState.Opening:
                case ConsoleState.Open:
                    SpriteBatch.Begin();
                    // Draw background.
                    if (BgRenderer.Texture != null)
                    {
                        BgRenderer.Draw();
                    }
                    else
                    {
                        SpriteBatch.Draw(
                            _whiteTexture,
                            WindowArea,
                            new RectangleF(
                                0,
                                0,
                                WindowArea.Width,
                                WindowArea.Height),
                            BackgroundColor);
                    }
                    // Draw bottom border if enabled (thickness larger than zero).
                    if (BottomBorderThickness > 0)
                        SpriteBatch.Draw(_whiteTexture,
                            new RectangleF(0, WindowArea.Bottom, WindowArea.Width, BottomBorderThickness),
                            BottomBorderColor);                                        
                    // Draw output and input strings.
                    ConsoleOutput.Draw();                    
                    ConsoleInput.Draw();
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
                    string cmd = ConsoleInput.Value;                    
                    // Determine if this is a line break or we should execute command straight away.
                    if (_actionDefinitions.BackwardTryGetValue(ConsoleAction.NextLineModifier, out modifier) &&
                        Input.IsKeyDown(modifier))
                    {                        
                        ConsoleOutput.AddCommandEntry(cmd);                        
                    } 
                    else
                    {
                        string executedCmd = cmd;
                        if (ConsoleOutput.HasCommandEntry)
                            executedCmd = ConsoleOutput.DequeueCommandEntry() + cmd;
                                                
                        // Replace our tab symbols with actual tab characters.
                        executedCmd = executedCmd.Replace(Tab, "\t");
                        // Log the command to be executed if logger is set.
                        LogInput?.Invoke(executedCmd);
                        // Execute command.
                        _commandInterpreter.Execute(ConsoleOutput, executedCmd);
                    }
                                        
                    // If the cmd matches the currently indexed historical entry then set a special flag
                    // which when moving backward in history, does not actually move backward, but will instead
                    // return the same entry that was returned before. This is similar to how Powershell and Cmd Prompt work.
                    if (_inputHistory.Count == 0 || _inputHistoryIndexer == int.MaxValue || !_inputHistory[_inputHistoryIndexer].Equals(cmd))
                        _inputHistoryIndexer = int.MaxValue;
                    else
                        _inputHistoryDoNotDecrement = true;

                    ConsoleInput.LastAutocompleteEntry = null;

                    // Find the last historical entry if any.
                    string lastHistoricalEntry = null;
                    if (_inputHistory.Count > 0)
                        lastHistoricalEntry = _inputHistory[_inputHistory.Count - 1];

                    // Only add current command to input history if it is not an empty string and
                    // does not match the last historical entry.
                    if (cmd != "" && !cmd.Equals(lastHistoricalEntry, StringComparison.Ordinal))
                        _inputHistory.Add(cmd);

                    ConsoleInput.Clear();                                        
                    ConsoleInput.Caret.MoveBy(int.MinValue);                    
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
                    if (hasModifier && !Input.IsKeyDown(modifier))
                        return ConsoleProcessResult.None;
                    bool canMoveBackwards = _actionDefinitions.BackwardTryGetValue(ConsoleAction.PreviousEntryModifier, out modifier);
                    _commandInterpreter.Autocomplete(ConsoleInput, !canMoveBackwards || !Input.IsKeyDown(modifier));
                    ConsoleInput.Caret.Index = ConsoleInput.Length;
                    _inputHistoryIndexer = int.MaxValue;
                    return ConsoleProcessResult.Break;
                case ConsoleAction.MoveLeft:
                    _actionDefinitions.BackwardTryGetValue(ConsoleAction.MoveByWordModifier, out modifier);
                    if (Input.IsKeyDown(modifier))
                        ConsoleInput.Caret.MoveToPreviousWord();
                    else
                        ConsoleInput.Caret.MoveBy(-1);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.MoveRight:
                    _actionDefinitions.BackwardTryGetValue(ConsoleAction.MoveByWordModifier, out modifier);
                    if (Input.IsKeyDown(modifier))
                        ConsoleInput.Caret.MoveToNextWord();
                    else
                        ConsoleInput.Caret.MoveBy(1);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.MoveToBeginning:
                    ConsoleInput.Caret.Index = 0;
                    return ConsoleProcessResult.Break;
                case ConsoleAction.MoveToEnd:
                    ConsoleInput.Caret.Index = ConsoleInput.Length;
                    return ConsoleProcessResult.Break;
                case ConsoleAction.DeletePreviousChar:
                    if (ConsoleInput.Length > 0 && ConsoleInput.Caret.Index > 0)
                    {                        
                        ConsoleInput.Remove(Math.Max(0, ConsoleInput.Caret.Index - 1), 1);
                        ResetLastHistoryAndAutocompleteEntries();
                    }
                    return ConsoleProcessResult.Break;
                case ConsoleAction.DeleteCurrentChar:
                    if (ConsoleInput.Length > ConsoleInput.Caret.Index)
                    {                        
                        ConsoleInput.Remove(ConsoleInput.Caret.Index, 1);
                        ResetLastHistoryAndAutocompleteEntries();
                    }
                    return ConsoleProcessResult.Break;
                case ConsoleAction.Paste:                    
                    _actionDefinitions.BackwardTryGetValue(ConsoleAction.CopyPasteModifier, out modifier);
                    if (!Input.IsKeyDown(modifier))
                        break;
                    // TODO: Enable clipboard pasting. How to approach this in a cross-platform manner?
                    //string clipboardVal = Clipboard.GetText(TextDataFormat.Text);                        
                    //_currentInput.Append(clipboardVal);
                    //MoveCaret(clipboardVal.Length);
                    return ConsoleProcessResult.Break;
                case ConsoleAction.Tab:
                    _actionDefinitions.BackwardTryGetValue(ConsoleAction.TabModifier, out modifier);
                    if (Input.IsKeyDown(modifier))
                        ConsoleInput.RemoveTab();
                    else
                        ConsoleInput.Write(Tab);
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

            ConsoleInput.Write(toUpper
                ? symbolPair.UppercaseSymbol
                : symbolPair.LowercaseSymbol);
            
            ResetLastHistoryAndAutocompleteEntries(); 
            return ConsoleProcessResult.Break;
        }

        private void ResetLastHistoryAndAutocompleteEntries()
        {
            ConsoleInput.LastAutocompleteEntry = null;
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
            ConsoleInput.LastAutocompleteEntry = null;
            ConsoleInput.Value = _inputHistory[_inputHistoryIndexer];
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
            SetWindowWidthAndHeight(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
#else
            SetWindowWidthAndHeight(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);            
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
            BackgroundColor = settings.BackgroundColor;            
            FontColor = settings.FontColor;
            HeightRatio = settings.HeightRatio;
            OpenCloseTransitionSeconds = settings.TimeToToggleOpenClose;
            RepeatingInputCooldown = settings.TimeToCooldownRepeatingInput;
            TimeUntilRepeatingInput = settings.TimeToTriggerRepeatingInput;                        
            Padding = settings.Padding;
            BottomBorderColor = settings.BottomBorderColor;
            BottomBorderThickness = settings.BottomBorderThickness;

            BgRenderer.SetDefault(settings);
            ConsoleInput.SetDefaults(settings);
            ConsoleOutput.SetDefaults(settings);
        }           
    }
}