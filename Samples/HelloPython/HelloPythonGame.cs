using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using QuakeConsole.Samples.Common;

namespace QuakeConsole.Samples.HelloPython
{
    public class HelloPythonGame : Game
    {                
        private readonly GraphicsDeviceManager _graphics;
        // We need a reference to console to open and close it.
        private readonly ConsoleComponent _console;

        private KeyboardState _previousKeyState;
        private KeyboardState _currentKeyState;

        public HelloPythonGame()
        {
            Window.Title = "Hello Python in MonoGame :)";
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720
            };            

            // Create a sample cube component.
            var cube = new CubeComponent(this);
            Components.Add(cube);

            // Create a console component.
            _console = new ConsoleComponent(this);
            Components.Add(_console);            

            // Create an interpreter - this will execute the commands typed into console.
            // In this case we are creating a python interpreter which allows python code to be executed.
            var interpreter = new PythonInterpreter();
            // Register the cube and the console itself as objects we can manipulate in-game.
            interpreter.AddVariable("cube", cube);
            interpreter.AddVariable("console", _console);            
            // Finally, tell the console to use this interpreter.
            _console.Interpreter = interpreter;

            // Add component for on-screen helper instructions.
            Components.Add(new InstructionsComponent(this));
        }

        public Keys ToggleConsoleKey { get; } = Keys.OemTilde;
        public Color BackgroundColor { get; } = new Color(new Vector3(0.125f));

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();

            // If toggle console key is pressed, toggle the console to open or close.
            if (IsKeyPressed(ToggleConsoleKey))
                _console.ToggleOpenClose();                        

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(BackgroundColor);
            base.Draw(gameTime);
        }

        private bool IsKeyPressed(Keys key) => _previousKeyState.IsKeyUp(key) && _currentKeyState.IsKeyDown(key);
    }
}
