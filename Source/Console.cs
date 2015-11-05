using System;
using System.Collections.Generic;
using QuakeConsole.Input;
using QuakeConsole.Output;
using QuakeConsole.Utilities;
#if MONOGAME
using Texture = Microsoft.Xna.Framework.Graphics.Texture2D;
using MathUtil = Microsoft.Xna.Framework.MathHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif

namespace QuakeConsole
{    
    internal class Console : IDisposable
    {
        private const string MeasureFontSizeSymbol = "x";

        public event EventHandler FontChanged;
        public event EventHandler PaddingChanged;
        public event EventHandler WindowAreaChanged;

        private readonly Timer _transitionTimer = new Timer { AutoReset = false };

        private ICommandInterpreter _commandInterpreter;
        private GraphicsDeviceManager _graphicsDeviceManager;        
                
        private SpriteFont _font;
        private RectangleF _windowArea;        
        private float _padding;
        private float _heightRatio;
        private string _tabSymbol;
        private string _newlineSymbol = Environment.NewLine;
        private bool _loaded;

        public Console()
        {            
            SetSettings(new ConsoleSettings());
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
                MeasureFontSize();
                CalculateMaxNumberOfVisibleLines();
                FontChanged?.Invoke(this, EventArgs.Empty);               
            }
        }

        public Vector2 FontSize { get; private set; }

        public Texture WhiteTexture { get; private set; }

        public string TabSymbol
        {
            get { return _tabSymbol; }
            set { _tabSymbol = value ?? ""; }
        }

        public string NewlineSymbol
        {
            get { return _newlineSymbol; }
            set { _newlineSymbol = value ?? ""; }
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
                    CalculateMaxNumberOfVisibleLines();
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

        public ConsoleState State { get; private set; } = ConsoleState.Closed;

        public Dictionary<char, float> CharWidthMap { get; } = new Dictionary<char, float>();

        public TexturedBackground BgRenderer { get; } = new TexturedBackground();
        public SpriteBatch SpriteBatch { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }

        public RectangleF WindowArea
        {
            get { return _windowArea; }
            set
            {
                value.Width = Math.Max(value.Width, 0);
                value.Height = Math.Max(value.Height, 0);
                _windowArea = value;
                CalculateMaxNumberOfVisibleLines();
                WindowAreaChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int LineIndexAfterInput => ConsoleInput.MultiLineInput.InputLines.Count;
        public int NumberOfAvailableLinesAfterInput => TotalNumberOfVisibleLines - ConsoleInput.MultiLineInput.InputLines.Count;
        public int TotalNumberOfVisibleLines { get; private set; }

        public void LoadContent(GraphicsDevice device, GraphicsDeviceManager deviceManager,
            SpriteFont font, ICommandInterpreter commandInterpreter)
        {
            Check.ArgumentNotNull(deviceManager, nameof(deviceManager), "Cannot instantiate the console without graphics device manager.");
            Check.ArgumentNotNull(font, nameof(font), "Cannot instantiate the console without a font.");

            Interpreter = commandInterpreter;
            GraphicsDevice = device;
            _graphicsDeviceManager = deviceManager;
            Font = font;

            SpriteBatch = new SpriteBatch(GraphicsDevice);

            _graphicsDeviceManager.PreparingDeviceSettings += OnPreparingDeviceChanged;
#if MONOGAME
            WhiteTexture = new Texture2D(GraphicsDevice, 2, 2, false, SurfaceFormat.Color);
            WhiteTexture.SetData(new[] { Color.White, Color.White, Color.White, Color.White });
#else                   
            WhiteTexture = Texture.New2D(GraphicsDevice, 2, 2, PixelFormat.R8G8B8A8_UNorm, new[] { Color.White, Color.White, Color.White, Color.White });            
#endif
            _loaded = true;

            MeasureFontSize();
            SetWindowWidthAndHeight();
            CalculateMaxNumberOfVisibleLines();

            ConsoleInput.LoadContent(this);
            ConsoleOutput.LoadContent(this);
            BgRenderer.LoadContent(this);
        }

        public void Dispose()
        {
            _graphicsDeviceManager.PreparingDeviceSettings -= OnPreparingDeviceChanged;

            SpriteBatch?.Dispose();
            WhiteTexture?.Dispose();
            BgRenderer.Dispose();
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
        
        public void Clear(ConsoleClearFlags flags = ConsoleClearFlags.All)
        {
            if ((flags & ConsoleClearFlags.OutputBuffer) != 0)
                ConsoleOutput.Clear();
            if ((flags & ConsoleClearFlags.InputBuffer) != 0)
                ConsoleInput.Clear();
            if ((flags & ConsoleClearFlags.InputHistory) != 0)
                ConsoleInput.InputHistory.Clear();
        }
        
        public void Reset()
        {
            Clear();
            SetSettings(new ConsoleSettings());
        }
        
        public void Update(float deltaSeconds)
        {

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
                            WhiteTexture,
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
                        SpriteBatch.Draw(
                            WhiteTexture,
                            new RectangleF(0, WindowArea.Bottom, WindowArea.Width, BottomBorderThickness),
                            BottomBorderColor);                                        
                    // Draw output and input strings.
                    ConsoleOutput.Draw();                    
                    ConsoleInput.Draw();
                    SpriteBatch.End();
                    break;
            }
        }

        private void UpdateTimer(float deltaSeconds, ConsoleState stateToSetWhenFinished)
        {
            _transitionTimer.Update(deltaSeconds);
            if (_transitionTimer.Finished)
                State = stateToSetWhenFinished;
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

        private void MeasureFontSize()
        {
            FontSize = Font.MeasureString(MeasureFontSizeSymbol);
        }

        private void SetSettings(ConsoleSettings settings)
        {
            BackgroundColor = settings.BackgroundColor;            
            FontColor = settings.FontColor;
            HeightRatio = settings.HeightRatio;
            OpenCloseTransitionSeconds = settings.TimeToToggleOpenClose;            
            Padding = settings.Padding;
            BottomBorderColor = settings.BottomBorderColor;
            BottomBorderThickness = settings.BottomBorderThickness;
            TabSymbol = settings.TabSymbol;

            BgRenderer.SetDefaults(settings);
            ConsoleInput.SetDefaults(settings);
            ConsoleOutput.SetDefaults(settings);
        }

        private void CalculateMaxNumberOfVisibleLines()
        {            
            // Take top padding into account and hide any row which is only partly visible.
            //_maxNumRows = Math.Max((int)((_console.WindowArea.Height - _console.Padding * 2) / Console.FontSize.Y) - 1, 0);            

            // Disregard top padding and allow any row which is only partly visible.
            TotalNumberOfVisibleLines = Math.Max((int)Math.Ceiling((WindowArea.Height - Padding) / FontSize.Y), 0);
        }
    }

    /// <summary>
    /// Defines which subparts of the <see cref="Console"/> to clear.
    /// </summary>
    [Flags]
    public enum ConsoleClearFlags
    {
        /// <summary>
        /// Does not clear anything.
        /// </summary>
        None = 0,
        /// <summary>
        /// Clears the text in the output part of the console.
        /// </summary>
        OutputBuffer = 1,
        /// <summary>
        /// Clears the text in the input part of the console and resets Caret position.
        /// </summary>
        InputBuffer = 2,
        /// <summary>
        /// Removes any history of user input.
        /// </summary>
        InputHistory = 4,
        /// <summary>
        /// Clears everything.
        /// </summary>
        All = OutputBuffer | InputBuffer | InputHistory
    }
}