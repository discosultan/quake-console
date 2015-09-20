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
        GraphicsDeviceManager graphics;        
        private readonly ConsoleComponent console;
        private readonly PythonInterpreter interpreter = new PythonInterpreter();

        private Cube cube;
       
        private KeyboardState _previousKeyState;
        private KeyboardState _currentKeyState;

        public SandboxGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            console = new ConsoleComponent(this);
            Components.Add(console);

            graphics.PreferredBackBufferWidth = 1240;
            graphics.PreferredBackBufferHeight = 768;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            console.LoadContent(Content.Load<SpriteFont>("Font"), interpreter);

            cube = new Cube(GraphicsDevice);
            interpreter.AddVariable("cube", cube);
            interpreter.AddVariable("console", console);
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

            if (IsKeyPressed(Keys.OemTilde))
                console.ToggleOpenClose();

            float deltaSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;

            cube.Update(deltaSeconds);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            cube.Draw();

            base.Draw(gameTime);
        }

        private bool IsKeyPressed(Keys key)
        {
            return _previousKeyState.IsKeyUp(key) && _currentKeyState.IsKeyDown(key);
        }
    }
}
