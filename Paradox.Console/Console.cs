using System;
using System.Collections.Generic;
using System.Text;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;

namespace Varus.Paradox.Console
{
    public class Console : GameSystemBase
    {
        private readonly BiDirectionalDictionary<Keys, ConsoleAction> _actionDefinitions = new BiDirectionalDictionary<Keys, ConsoleAction>
        {
            { Keys.Enter, ConsoleAction.ExecuteCommand }
        };

        private readonly Dictionary<Keys, SymbolPair> _symbolDefinitions = new Dictionary<Keys, SymbolPair>
        {
            { Keys.D1, new SymbolPair("1", "!") },
            { Keys.D2, new SymbolPair("2", "@") },
            { Keys.D2, new SymbolPair("3", "#") },
        };        

        private readonly IConsoleCommandInterpreter _commandInterpreter;        

        private Texture2D _backgroundTexture;
        private SpriteBatch _spriteBatch;
        private readonly InputManager _inputManager;
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private SpriteFont _font;
        private Vector2 _fontSize;

        private int _maxNumViewBufferRows;
        private readonly List<string> _viewBuffer = new List<string>();
        private readonly StringBuilder _inputBuffer = new StringBuilder();
        private readonly Timer _transitionTimer = new Timer { AutoReset = false };        
        private ConsoleState _state = ConsoleState.Closed;
        private RectangleF _windowArea;
        private Caret _caret;

        public Console(IServiceRegistry registry, IConsoleCommandInterpreter commandInterpreter, SpriteFont font)
            : base(registry)
        {            
            _commandInterpreter = commandInterpreter;
            _graphicsDeviceManager = (GraphicsDeviceManager)registry.GetSafeServiceAs<IGraphicsDeviceManager>();
            _inputManager = (InputManager)registry.GetSafeServiceAs<IInputManager>();
            Font = font;            

            // Defaults.
            BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            FontColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
            OpenCloseTransitionSeconds = 0.25f;
            HeightRatio = 0.4f;
            Padding = 0;
            InputPrefix = ">";
        }        

        public SpriteFont Font
        {
            get { return _font; }
            set
            {
                _font = value;
                _fontSize = _font.MeasureString("M");
            }
        }
        public Color BackgroundColor { get; set; }
        public Color FontColor { get; set; }
        public float OpenCloseTransitionSeconds
        {
            get { return _transitionTimer.TargetTime; }
            set { _transitionTimer.TargetTime = value; }
        }
        public float HeightRatio { get; set; }
        public string InputPrefix { get; set; }
        public float Padding { get; set; }

        protected override void LoadContent()
        {
            _graphicsDeviceManager.PreparingDeviceSettings += SetWindowWidthAndHeight;

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _backgroundTexture = Texture2D.New(GraphicsDevice, 1, 1, PixelFormat.R32G32B32A32_Float);
            _caret = new Caret(_spriteBatch, _font);            
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

        public override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.Elapsed.TotalSeconds; 
            switch (_state)
            {
                case ConsoleState.Closing:
                    UpdateTimer(deltaSeconds, ConsoleState.Closed);
                    _windowArea.Y = -_windowArea.Height * _transitionTimer.Progress;
                    _caret.Update(deltaSeconds);
                    break;
                case ConsoleState.Opening:
                    UpdateTimer(deltaSeconds, ConsoleState.Open);
                    _windowArea.Y = -_windowArea.Height + _windowArea.Height * _transitionTimer.Progress;                    
                    goto case ConsoleState.Open;
                case ConsoleState.Open:
                    _caret.Update(deltaSeconds);
                    HandleInput();
                    break;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            switch (_state)
            {
                case ConsoleState.Closing:
                case ConsoleState.Opening:
                case ConsoleState.Open:
                    _spriteBatch.Begin();
                    _spriteBatch.Draw(
                        _backgroundTexture,
                        _windowArea,
                        BackgroundColor);
                    // Draw console buffer.
                    for (int i = 0; i < _viewBuffer.Count; ++i)
                    {
                        var viewPosition = new Vector2(Padding, _windowArea.Y + i * (_fontSize.Y));
                        string line = _viewBuffer[i];
                        _spriteBatch.DrawString(_font, line, viewPosition, FontColor);
                    }
                    //// Draw input symbol.
                    //_inputPosition.X = MarginLeft;
                    //_spriteBatch.DrawString(_font, InputPrefix, _inputPosition, FontColor);
                    //// Draw user input.
                    //_inputPosition.X += _inputPrefixWidth;
                    //_inputPosition.Y = _windowArea.Y + _windowArea.Height - _fontHeight;
                    //_spriteBatch.DrawString(_font, _currentInput, _inputPosition, FontColor);
                    //// Draw caret.                    
                    //_inputPosition.X = MarginLeft + _inputPrefixWidth * (_caret.Index + 1);
                    //_caret.Draw(ref _inputPosition, FontColor);
                    _spriteBatch.End();
                    break;
            }
        }

        protected override void UnloadContent()
        {
            _graphicsDeviceManager.PreparingDeviceSettings -= SetWindowWidthAndHeight;

            _spriteBatch.Dispose();
            _backgroundTexture.Dispose();
        }


        #region Input handling

        //private readonly List<Keys> _pressedKeys = new List<Keys>();
        private void HandleInput()
        {
            List<Keys> downKeys = _inputManager.KeyDown;
            foreach (Keys key in downKeys)
            {
                ConsoleKeyProcessResult processResult = ProcessSpecialKey(key);
                if (processResult == ConsoleKeyProcessResult.Continue) continue;
                if (processResult == ConsoleKeyProcessResult.Break) break;
                processResult = ProcessRegularKey(key);
                if (processResult == ConsoleKeyProcessResult.Break) break;
            }
        }

        private ConsoleKeyProcessResult ProcessSpecialKey(Keys key)
        {
            ConsoleAction action;
            if (!_actionDefinitions.ForwardTryGetValue(key, out action))
                return ConsoleKeyProcessResult.None;

            switch (action)
            {
                case ConsoleAction.ExecuteCommand:
                    if (_inputManager.IsKeyPressed(key))
                    {
                        AppendToViewBuffer(_commandInterpreter.Execute(_inputBuffer.ToString()));
                        _inputBuffer.Clear();
                        _caret.Index = 0;
                        return ConsoleKeyProcessResult.Break;
                    }
                    break;
            }

            return ConsoleKeyProcessResult.None;
        }

        private ConsoleKeyProcessResult ProcessRegularKey(Keys key)
        {
            SymbolPair symbolPair;
            if (!_symbolDefinitions.TryGetValue(key, out symbolPair))
                return ConsoleKeyProcessResult.None;

            Keys uppercaseModifier;
            if (!_actionDefinitions.BackwardTryGetValue(ConsoleAction.UppercaseModifier, out uppercaseModifier))
                uppercaseModifier = Keys.None;

            AppendToInputBuffer(_inputManager.IsKeyDown(uppercaseModifier)
                ? symbolPair.UppercaseSymbol
                : symbolPair.LowercaseSymbol);

            return ConsoleKeyProcessResult.None;
        }

        #endregion


        private void AppendToViewBuffer(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            RemoveOverflowBufferEntries(_viewBuffer.Count + 1);
            _viewBuffer.Add(message);
        }

        private void AppendToInputBuffer(string symbol)
        {
            if (string.IsNullOrEmpty(symbol)) return;
            _inputBuffer.Append(symbol);
        }

        private void RemoveOverflowBufferEntries(int numBufferRows)
        {
            int diff = _maxNumViewBufferRows - numBufferRows;
            if (diff >= 0) return;
            for (int i = Math.Abs(diff) - 1; i >= 0; --i)
            {
                if (_viewBuffer.Count > 0)
                {
                    _viewBuffer.RemoveAt(i);
                }
            }
        }

        private void UpdateTimer(float deltaSeconds, ConsoleState stateToSetWhenFinished)
        {
            _transitionTimer.Update(deltaSeconds);
            if (_transitionTimer.Finished)
            {
                _state = stateToSetWhenFinished;
            }
        }

        private void SetWindowWidthAndHeight(object sender, EventArgs args)
        {
            _windowArea.Width = GraphicsDevice.BackBuffer.Width;
            _windowArea.Height = GraphicsDevice.BackBuffer.Height * HeightRatio;
            _windowArea.Y = -_windowArea.Height;
            _maxNumViewBufferRows = (int)(_windowArea.Height / _fontSize.Y) - 1;
            RemoveOverflowBufferEntries(_viewBuffer.Count);
        }
    }
}
