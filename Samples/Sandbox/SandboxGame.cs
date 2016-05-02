using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace QuakeConsole.Samples.Sandbox
{    
    public class SandboxGame : Game
    {        
        private const Keys ToggleConsole = Keys.OemTilde;
        private const float ConsoleBackgroundSpeedFactor = 1/24f;        
        private static readonly Color BackgroundColor = new Color(new Vector3(0.125f));
        private static readonly Color ForegroundColor = Color.White;
        private static readonly Color SelectionColor = Color.DarkGray;
        private static readonly Vector2 ConsoleBackgroundTiling = new Vector2(2.5f, 1.5f);

        private readonly GraphicsDeviceManager _graphics;

        private readonly ConsoleComponent _console;
        private readonly PythonInterpreter _pythonInterpreter = new PythonInterpreter();
        private readonly ManualInterpreter _manualInterpreter = new ManualInterpreter();
        private readonly RoslynInterpreter _roslynInterpreter = new RoslynInterpreter();
        private readonly CameraControllerComponent _camera;

        private SpriteBatch _spriteBatch;        

        private CubeComponent _cube;

        private SpriteFont _lucidaConsole;
        private SpriteFont _arial;                
        
        private Matrix _consoleBgTransform = Matrix.Identity;

        private KeyboardState _previousKeyState;
        private KeyboardState _currentKeyState;

        public SandboxGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _camera = new CameraControllerComponent(this);            
            _cube = new CubeComponent(this);
            Components.Add(_camera);
            Components.Add(_cube);

            _console = new ConsoleComponent(this)
            {
                Padding = 10.0f,
                FontColor = Color.White,
                InputPrefixColor = Color.White,
                BackgroundColor = Color.White,                
                BottomBorderThickness = 4.0f,
                BottomBorderColor = Color.Red,
                SelectionColor = SelectionColor,                
                LogInput = cmd => Debug.WriteLine(cmd) // Logs input commands to VS output window.
            };            
            Components.Add(_console);            

            // Add search path for IronPython standard library. This is so that Python engine knows where to load modules from.
            _pythonInterpreter.AddSearchPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lib\\"));

            // Import threading module and Timer function.
            _pythonInterpreter.RunScript("import threading");
            _pythonInterpreter.RunScript("import random");
            _pythonInterpreter.RunScript("from threading import Timer");            

            // There's a bug when trying to change resolution during window resize.
            // https://github.com/mono/MonoGame/issues/3572
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;            
            Window.AllowUserResizing = false;            

            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);            

            _arial = Content.Load<SpriteFont>("arial");
            _lucidaConsole = Content.Load<SpriteFont>("lucida_console");

            // Set console's font and interpreter.             
            _console.Interpreter = _pythonInterpreter;
            _console.BackgroundTexture = Content.Load<Texture2D>("console");            

            // Register variables and types of interest with the Python interpreter.
            _pythonInterpreter.AddVariable("cube", _cube);
            _pythonInterpreter.AddVariable("console", _console);
            _pythonInterpreter.AddVariable("pythonInterpreter", _pythonInterpreter);
            _pythonInterpreter.AddVariable("manualInterpreter", _manualInterpreter);
            _pythonInterpreter.AddVariable("roslynInterpreter", _roslynInterpreter);
            _pythonInterpreter.AddType(typeof (Utilities));

            // Register commands with the manual interpreter.
            _manualInterpreter.RegisterCommand("Set-Console-Interpreter-Python", _ =>
            {
                _console.Interpreter = _pythonInterpreter;
                return "Console interpreter switched to Python";
            });
            _manualInterpreter.RegisterCommand("Set-Console-Interpreter-Roslyn", _ =>
            {
                _console.Interpreter = _roslynInterpreter;
                return "Console interpreter switched to Roslyn";
            });
            _manualInterpreter.RegisterCommand("Set-Cube-Position", args => _cube.Position = args.ToVector3());
            _manualInterpreter.RegisterCommand("Set-Cube-Scale", args => _cube.Scale = args.ToVector3());
            _manualInterpreter.RegisterCommand("Set-Cube-Rotation", args => _cube.Rotation = args.ToVector3());
            _manualInterpreter.RegisterCommand("Set-Cube-RotationSpeed", args => _cube.RotationSpeed = args.ToVector3());

            // Register variables with the roslyn interpreter.            
            _roslynInterpreter.AddVariable("cube", _cube);
            _roslynInterpreter.AddVariable("console", _console);
            _roslynInterpreter.AddVariable("pythonInterpreter", _pythonInterpreter);
            _roslynInterpreter.AddVariable("manualInterpreter", _manualInterpreter);
            _roslynInterpreter.AddVariable("roslynInterpreter", _roslynInterpreter);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();

            if (IsKeyPressed(ToggleConsole))
                _console.ToggleOpenClose();            
            
            _camera.Enabled = !_console.IsAcceptingInput;
            _cube.View = _camera.View;
            _cube.Projection = _camera.Projection;

            _consoleBgTransform = Matrix.CreateScale(new Vector3(ConsoleBackgroundTiling, 0)) *
                                  Matrix.CreateTranslation((float)gameTime.TotalGameTime.TotalSeconds * ConsoleBackgroundSpeedFactor, 0, 0);
            _console.BackgroundTextureTransform = _consoleBgTransform;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(BackgroundColor);

            _spriteBatch.Begin();
            DrawInstructions();
            _spriteBatch.End();

            base.Draw(gameTime);
        }        

        private bool IsKeyPressed(Keys key) => _previousKeyState.IsKeyUp(key) && _currentKeyState.IsKeyDown(key);        

        private void DrawInstructions()
        {
            _spriteBatch.DrawString(
                _lucidaConsole,
                $"Press {ToggleConsole} to open console. Use {Keys.LeftControl} + {Keys.Space} to autocomplete.",
                new Vector2(10, GraphicsDevice.Viewport.Height - 50),
                ForegroundColor);
            _spriteBatch.DrawString(
                _lucidaConsole,
                $"Use {_camera.MoveForwardKey} {_camera.MoveLeftKey} {_camera.MoveBackwardKey} {_camera.MoveRightKey} {_camera.MoveUpKey} {_camera.MoveDownKey} and hold mouse right button to navigate the camera.",
                new Vector2(10, GraphicsDevice.Viewport.Height - 25),
                ForegroundColor);
        }
    }
}
