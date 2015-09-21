#if MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace QuakeConsole
{
    public class ConsoleComponent : DrawableGameComponent
    {
        private readonly Console _console = new Console();

        private SpriteFont _font;
        private ICommandInterpreter _commandInterpreter;        

        public ConsoleComponent(Game game) : base(game)
        {            
            Enabled = true;
            Visible = true;
        }        

        public void LoadContent(SpriteFont font, ICommandInterpreter commandInterpreter = null)
        {
            _font = font;
            _commandInterpreter = commandInterpreter;

            _console.LoadContent(
                GraphicsDevice,
                (GraphicsDeviceManager)Game.Services.GetService<IGraphicsDeviceManager>(),
                _font,
                _commandInterpreter);
        }

        public Color BackgroundColor
        {
            get {  return _console.BackgroundColor; }  
            set { _console.BackgroundColor = value; }
        }

        public Color FontColor
        {
            get { return _console.FontColor; }
            set { _console.FontColor = value; }
        }

        public override void Update(GameTime gameTime)
        {
            if (Enabled)
                _console.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
        }

        public override void Draw(GameTime gameTime)
        {            
            if (Visible)
                _console.Draw();
        }

        public void ToggleOpenClose()
        {
            _console.ToggleOpenClose();
        }
    }
}
#endif