using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace QuakeConsole.Samples.HelloPython
{
    class InstructionsComponent : DrawableGameComponent
    {
        private static readonly Color ForegroundColorDefault = Color.White;
        private static readonly Color ForegroundColorHighlight = Color.Yellow;

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
            const float margin = 10;
            float sizeY = _font.LineSpacing;
            
            // Render key instructions.
            float progressX = margin;
            float progressY = GraphicsDevice.Viewport.Height - margin - sizeY;
            for (int i = _instructionPrefixes.Length - 1; i >= 0; i--)
            {                               
                _spriteBatch.DrawString(_font, _instructionPrefixes[i], new Vector2(progressX, progressY), ForegroundColorDefault);
                progressX += _font.MeasureString(_instructionPrefixes[i]).X;
                _spriteBatch.DrawString(_font, _instructionKeys[i], new Vector2(progressX, progressY), ForegroundColorHighlight);
                progressX += _font.MeasureString(_instructionKeys[i]).X;
                _spriteBatch.DrawString(_font, _instructionSuffixes[i], new Vector2(progressX, progressY), ForegroundColorDefault);

                progressY -= sizeY;
                progressX = margin;
            }

            // Render sample commands.
            progressY = GraphicsDevice.Viewport.Height - margin - sizeY;
            for (int i = _sampleCommands.Length - 1; i >= 0; i--)
            {
                progressX = GraphicsDevice.Viewport.Width - margin - _font.MeasureString(_sampleCommands[i]).X;
                _spriteBatch.DrawString(_font, _sampleCommands[i], new Vector2(progressX, progressY), ForegroundColorHighlight);

                progressY -= sizeY;
            }
            string commandsTitle = "Sample commands to try:";
            progressX = GraphicsDevice.Viewport.Width - margin - _font.MeasureString(commandsTitle).X;
            _spriteBatch.DrawString(_font, commandsTitle, new Vector2(progressX, progressY), ForegroundColorDefault);
        }

        string[] _instructionPrefixes =
        {
            "Open/close console [",
            "Execute command [",
            "Autocomplete entry [",
            "Add newline ["
        };
        string[] _instructionKeys =
        {
            "Tilde",
            "Enter",
            "Control+Space",
            "Shift+Enter"
        };
        string[] _instructionSuffixes = { "]", "]", "]", "]" };

        string[] _sampleCommands =
        {
            "cube.RotationSpeed=Vector3(5)",
            "cube.Scale=Vector3(1,1,3)",
            "console.BackgroundColor=Color(0,100,100,100)"
        };
    }
}
