using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using QuakeConsole;
using Common;

namespace HelloPython
{
    public class HelloPythonGame : Game
    {        
        private const Keys ToggleConsole = Keys.OemTilde;        
        private static readonly Color BackgroundColor = new Color(0x1d, 0x1d, 0x1d, 0xff);
        private static readonly Color ForegroundColor = Color.White;

        private readonly GraphicsDeviceManager _graphics;

        private readonly ConsoleComponent _console;
        private readonly PythonInterpreter _pythonInterpreter = new PythonInterpreter();        

        private SpriteBatch _spriteBatch;        

        private CubeComponent _cube;

        private SpriteFont _lucidaConsole;
        private SpriteFont _arial;

        private KeyboardState _previousKeyState;
        private KeyboardState _currentKeyState;

        public HelloPythonGame()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720
            };
            IsMouseVisible = true;
            Content.RootDirectory = "Content";

            _cube = new CubeComponent(this)
            {
                Projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4,
                    _graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight,
                    0.1f, 1000),
                View = Matrix.CreateLookAt(
                    new Vector3(0, 5, 10), 
                    Vector3.Zero, 
                    new Vector3(0, 0.9238795f, -0.3826835f))
            };
            Components.Add(_cube);

            _console = new ConsoleComponent(this);
            Components.Add(_console);                                    
        }
        
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);            

            _arial = Content.Load<SpriteFont>("arial");
            _lucidaConsole = Content.Load<SpriteFont>("lucida_console");                        
            
            _console.LoadContent(_arial, _pythonInterpreter);                              

            // Register variables and types of interest with the Python interpreter.
            _pythonInterpreter.AddVariable("cube", _cube);
            _pythonInterpreter.AddVariable("console", _console);
            _pythonInterpreter.AddVariable("pythonInterpreter", _pythonInterpreter);
        }
        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();

            if (IsKeyPressed(ToggleConsole))
                _console.ToggleOpenClose();                        

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
            //_spriteBatch.DrawString(
            //    _lucidaConsole,
            //    $"Use {_camera.MoveForwardKey} {_camera.MoveLeftKey} {_camera.MoveBackwardKey} {_camera.MoveRightKey} {_camera.MoveUpKey} {_camera.MoveDownKey} and hold mouse right button to navigate the camera.",
            //    new Vector2(10, GraphicsDevice.Viewport.Height - 25),
            //    ForegroundColor);
        }
    }
}
