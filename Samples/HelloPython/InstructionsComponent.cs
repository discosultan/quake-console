using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace QuakeConsole.Samples.HelloPython
{
    class InstructionsComponent : DrawableGameComponent
    {
        private static readonly Color ForegroundColor = Color.White;

        private readonly HelloPythonGame _game;
        private SpriteBatch _spriteBatch;    
        private SpriteFont _font;        

        public InstructionsComponent(HelloPythonGame game) : base(game)
        {
            _game = game;
            game.Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);            
            _font = Game.Content.Load<SpriteFont>("instructions");
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            DrawInstructions();
            _spriteBatch.End();
        }

        private void DrawInstructions()
        {
            _spriteBatch.DrawString(
                _font,
                $"Press {_game.ToggleConsoleKey} to open console. Use {Keys.LeftControl} + {Keys.Space} to autocomplete.",
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
