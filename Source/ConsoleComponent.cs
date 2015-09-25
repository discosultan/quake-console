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
                Game.Content,
                _font,
                _commandInterpreter);
        }               

        public bool IsAcceptingInput => _console.IsAcceptingInput;

        public ICommandInterpreter Interpreter
        {
            get { return _console.Interpreter; }
            set { _console.Interpreter = value; }
        }

        public float Padding
        {
            get { return _console.Padding; }
            set { _console.Padding = value; }
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

        public float TimeToToggleOpenClose
        {
            get { return _console.OpenCloseTransitionSeconds; }
            set { _console.OpenCloseTransitionSeconds = value; }
        }

        //public float TimeToTriggerRepeatingInput
        //{
        //    get { return _console.TimeUntilRepeatingInput; }
        //    set { _console.TimeUntilRepeatingInput = value; }
        //}

        //public float TimeToCooldownRepeatingInput
        //{
        //    get { return _console.TimeUntilRepeatingInput; }
        //    set { _console.TimeUntilRepeatingInput = value; }
        //}

        public float HeightRatio
        {
            get { return _console.HeightRatio; }
            set { _console.HeightRatio = value; }
        }

        public string InputPrefix
        {
            get { return _console.ConsoleInput.InputPrefix; }
            set { _console.ConsoleInput.InputPrefix = value; }
        }

        public Color InputPrefixColor
        {
            get { return _console.ConsoleInput.InputPrefixColor; }
            set { _console.ConsoleInput.InputPrefixColor = value; }
        }

        public string CaretSymbol
        {
            get { return _console.ConsoleInput.Caret.Symbol; }
            set { _console.ConsoleInput.Caret.Symbol = value; }
        }

        public float CaretBlinkingInterval
        {
            get { return _console.ConsoleInput.Caret.BlinkIntervalSeconds; }
            set { _console.ConsoleInput.Caret.BlinkIntervalSeconds = value; }
        }

        public bool BottomBorderEnabled
        {
            get { return _console.BottomBorderEnabled; }
            set { _console.BottomBorderEnabled = value; }
        }

        public Color BottomBorderColor
        {
            get { return _console.BottomBorderColor; }
            set { _console.BottomBorderColor = value; }
        }

        public float BottomBorderThickness
        {
            get { return _console.BottomBorderThickness; }
            set { _console.BottomBorderThickness = value; }
        }

        public Texture2D BackgroundTexture
        {
            get { return _console.BgRenderer.Texture; }
            set { _console.BgRenderer.Texture = value; }
        }

        public Vector2 BackgroundTextureScale
        {
            get { return _console.BgRenderer.TextureScale; }
            set { _console.BgRenderer.TextureScale = value; }
        }

        public Matrix BackgroundTextureTransform
        {
            get { return _console.BgRenderer.TextureTransform; }
            set { _console.BgRenderer.TextureTransform = value; }
        }

        public void ToggleOpenClose()
        {
            _console.ToggleOpenClose();
        }

        public void Clear(ConsoleClearFlags clearFlags = ConsoleClearFlags.All) => _console.Clear(clearFlags);

        public void Reset() => _console.Reset();

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
    }
}
#endif