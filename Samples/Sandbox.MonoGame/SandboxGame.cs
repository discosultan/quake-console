using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using QuakeConsole;

namespace Sandbox
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class SandboxGame : Game
    {
        private const Keys ToggleConsole = Keys.OemTilde;
        private static readonly Color BackgroundColor = Color.LightSlateGray;

        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private SpriteFont _font;
        private readonly ConsoleComponent _console;
        private readonly PythonInterpreter _interpreter = new PythonInterpreter();

        private Cube _cube;
       
        private KeyboardState _previousKeyState;
        private KeyboardState _currentKeyState;

        public SandboxGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _console = new ConsoleComponent(this)
            {
                Padding = 10.0f,
                FontColor = Color.White,
                InputPrefixColor = Color.White,
                BottomBorderEnabled = true,
                BottomBorderThickness = 4.0f,
                BottomBorderColor = Color.Red
            };
            Components.Add(_console);

            // There's a bug when trying to change resolution during window resize.
            // https://github.com/mono/MonoGame/issues/3572
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 768;
            Window.AllowUserResizing = false;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _font = Content.Load<SpriteFont>("arial");
            _console.LoadContent(_font, _interpreter);

            _cube = new Cube(GraphicsDevice);
            _interpreter.AddVariable("cube", _cube);
            _interpreter.AddVariable("console", _console);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();

            if (IsKeyPressed(ToggleConsole))
                _console.ToggleOpenClose();

            float deltaSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;

            _cube.Update(deltaSeconds);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(BackgroundColor);

            _cube.Draw();
            _spriteBatch.Begin();
            DrawInstructions();
            _spriteBatch.End();

            base.Draw(gameTime);
        }        

        private bool IsKeyPressed(Keys key)
        {
            return _previousKeyState.IsKeyUp(key) && _currentKeyState.IsKeyDown(key);
        }

        private void DrawInstructions()
        {
            _spriteBatch.DrawString(
                _font,
                $"Press {ToggleConsole} to open console. Use {Keys.LeftControl} + {Keys.Space} to autocomplete.",
                new Vector2(10, GraphicsDevice.Viewport.Height - 25),
                Color.Yellow);
        }
    }
}
