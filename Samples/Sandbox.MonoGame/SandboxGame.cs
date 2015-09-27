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
        private const float ConsoleBackgroundSpeedFactor = 1/24f;        
        private static readonly Color BackgroundColor = Color.LightSlateGray;
        private static readonly Vector2 ConsoleBackgroundTiling = new Vector2(2.5f, 1.5f);

        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private BasicEffect _effect;

        private SpriteFont _lucidaConsole;
        private SpriteFont _arial;

        private Matrix _consoleBgTransform = Matrix.Identity;
        private readonly ConsoleComponent _console;
        private readonly PythonInterpreter _interpreter = new PythonInterpreter();

        private readonly CameraControllerComponent _camera;
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
                BottomBorderThickness = 4.0f,
                BottomBorderColor = Color.Red
            };
            Components.Add(_console);

            _camera = new CameraControllerComponent(this);
            Components.Add(_camera);

            // There's a bug when trying to change resolution during window resize.
            // https://github.com/mono/MonoGame/issues/3572
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 768;
            Window.AllowUserResizing = false;

            IsMouseVisible = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _effect = new BasicEffect(GraphicsDevice) { VertexColorEnabled = true };

            _arial = Content.Load<SpriteFont>("arial");
            _lucidaConsole = Content.Load<SpriteFont>("lucida_console");

            _camera.LoadContent();

            _console.LoadContent(_arial, _interpreter);
            _console.BackgroundTexture = Content.Load<Texture2D>("console");
            _console.BackgroundTextureScale = new Vector2(2.0f);
            _console.BackgroundColor = Color.White;

            _cube = new Cube(GraphicsDevice, _effect);
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
            
            _camera.Enabled = !_console.IsAcceptingInput;
            _cube.Update(deltaSeconds);            

            _consoleBgTransform = Matrix.CreateScale(new Vector3(ConsoleBackgroundTiling, 0)) *
                                  Matrix.CreateTranslation((float)gameTime.TotalGameTime.TotalSeconds * ConsoleBackgroundSpeedFactor, 0, 0);
            _console.BackgroundTextureTransform = _consoleBgTransform;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(BackgroundColor);

            _effect.View = _camera.View;
            _effect.Projection = _camera.Projection;

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
                _lucidaConsole,
                $"Press {ToggleConsole} to open console. Use {Keys.LeftControl} + {Keys.Space} to autocomplete.",
                new Vector2(10, GraphicsDevice.Viewport.Height - 25),
                Color.Yellow);
        }
    }
}
