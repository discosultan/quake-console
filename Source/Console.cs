using System;
using System.Collections.Generic;
using System.Linq;
using QuakeConsole.Features;
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
    internal partial class Console : IDisposable
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
        private bool _loaded;                    

        public Console()
        {            
            SetDefaults(new ConsoleSettings());
        }

        public void LoadContent(GraphicsDevice device, GraphicsDeviceManager deviceManager,
            SpriteFont font, ICommandInterpreter commandInterpreter)
        {
            Check.ArgumentNotNull(deviceManager, nameof(deviceManager), "Cannot instantiate the console without graphics device manager.");
            Check.ArgumentNotNull(font, nameof(font), "Cannot instantiate the console without a font.");

            _commandInterpreter = commandInterpreter ?? new StubCommandInterpreter();
            GraphicsDevice = device;
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
            InputHistory.LoadContent(this);
            Autocompletion.LoadContent(this);
            CopyPasting.LoadContent(this);
            Movement.LoadContent(this);
            Tabbing.LoadContent(this);
            Deletion.LoadContent(this);
            CommandExecution.LoadContent(this);
        }

        public void Dispose()
        {
            _graphicsDeviceManager.PreparingDeviceSettings -= OnPreparingDeviceChanged;

            SpriteBatch?.Dispose();
            _whiteTexture?.Dispose();
            BgRenderer.Dispose();
        }

        public ICommandInterpreter Interpreter
        {
            get { return _commandInterpreter; }
            set { _commandInterpreter = value ?? new StubCommandInterpreter(); }
        }

        public ConsoleInput ConsoleInput { get; } = new ConsoleInput();

        public ConsoleOutput ConsoleOutput { get; } = new ConsoleOutput();        
        
        public bool IsVisible => State != ConsoleState.Closed;
        
        public bool IsAcceptingInput => State == ConsoleState.Open || State == ConsoleState.Opening;                
        
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
            set { _symbolDefinitions = value ?? new Dictionary<Keys, SymbolPair>(); }
        }

#if MONOGAME
        internal InputManager Input { get; } = new InputManager();
#endif

        internal InputHistory InputHistory { get; } = new InputHistory();
        internal Autocompletion Autocompletion { get; } = new Autocompletion();
        internal CopyPasting CopyPasting { get; } = new CopyPasting();
        internal Movement Movement { get; } = new Movement();
        internal Tabbing Tabbing { get; } = new Tabbing();
        internal Deletion Deletion { get; } = new Deletion();
        internal CommandExecution CommandExecution { get; } = new CommandExecution();

        internal ConsoleState State { get; private set; } = ConsoleState.Closed;

        internal Dictionary<char, float> CharWidthMap { get; } = new Dictionary<char, float>();

        internal TexturedBackground BgRenderer { get; } = new TexturedBackground();
        internal SpriteBatch SpriteBatch { get; private set; }
        internal GraphicsDevice GraphicsDevice { get; private set; }

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
            switch (State)
            {
                case ConsoleState.Closed:
                    State = ConsoleState.Opening;
                    _transitionTimer.Reset();
                    break;
                case ConsoleState.Open:
                    State = ConsoleState.Closing;
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
            InputHistory.Clear(clearFlags);
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
            switch (State)
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
                    ConsoleInput.Update(deltaSeconds);
                    break;
            }
        }

        public void Draw()
        {
            switch (State)
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
            foreach (KeyEvent keyEvent in Input.KeyEvents)
            {
                // We are only interested in key presses.
                if (keyEvent.Type == KeyEventType.Pressed)
                    if (HandleKey(keyEvent.Key))
                        break;
            }
        }

        internal bool HandleKey(Keys key)
        {
            bool processedKey = ProcessSpecialKey(key);
            if (processedKey)
                return true;
            processedKey = ProcessRegularKey(key);
            return processedKey;
        }

        private bool ProcessSpecialKey(Keys key)
        {
            ConsoleAction action;
            if (!ActionDefinitions.ForwardTryGetValue(key, out action))
                return false;

            bool hasProcessedKey =  InputHistory.ProcessAction(action);
            hasProcessedKey =       Autocompletion.ProcessAction(action)    || hasProcessedKey;
            hasProcessedKey =       CopyPasting.ProcessAction(action)       || hasProcessedKey;
            hasProcessedKey =       Movement.ProcessAction(action)          || hasProcessedKey;
            hasProcessedKey =       Tabbing.ProcessAction(action)           || hasProcessedKey;
            hasProcessedKey =       Deletion.ProcessAction(action)          || hasProcessedKey;
            hasProcessedKey =       CommandExecution.ProcessAction(action)  || hasProcessedKey;

            return hasProcessedKey;
        }

        private bool ProcessRegularKey(Keys key)
        {
            SymbolPair symbolPair;
            if (!_symbolDefinitions.TryGetValue(key, out symbolPair))
                return false;

            InputHistory.ProcessSymbol(symbolPair);
            Autocompletion.ProcessSymbol(symbolPair);

            List<Keys> uppercaseModifiers;
            ActionDefinitions.BackwardTryGetValues(ConsoleAction.UppercaseModifier, out uppercaseModifiers);

            bool toUpper = uppercaseModifiers != null && uppercaseModifiers.Any(x => Input.IsKeyDown(x));

            ConsoleInput.Write(toUpper
                ? symbolPair.UppercaseSymbol
                : symbolPair.LowercaseSymbol);
            
            return true;
        }

        

#endregion       


        private void UpdateTimer(float deltaSeconds, ConsoleState stateToSetWhenFinished)
        {
            _transitionTimer.Update(deltaSeconds);
            if (_transitionTimer.Finished)
            {
                State = stateToSetWhenFinished;
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
            switch (State)
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

        private void SetDefaults(ConsoleSettings settings)
        {
            BackgroundColor = settings.BackgroundColor;            
            FontColor = settings.FontColor;
            HeightRatio = settings.HeightRatio;
            OpenCloseTransitionSeconds = settings.TimeToToggleOpenClose;            
            Padding = settings.Padding;
            BottomBorderColor = settings.BottomBorderColor;
            BottomBorderThickness = settings.BottomBorderThickness;

            BgRenderer.SetDefault(settings);
            ConsoleInput.SetDefaults(settings);
            ConsoleOutput.SetDefaults(settings);
        }
    }
}