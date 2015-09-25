using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace QuakeConsole
{
    internal class BackgroundRenderer
    {
        private Console _console;
        private Effect _bgEffect;
        private EffectParameter _bgEffectTexture;
        private EffectParameter _bgEffectWvpTransform;
        private EffectParameter _bgEffectTexTransform;
        private VertexBuffer _vertices;

        private Matrix _wvp;

        public void LoadContent(Console console)
        {
            _console = console;
            _console.WindowAreaChanged += (s, e) => CreateWvp();
            CreateWvp();
            _bgEffect = console.Content.Load<Effect>("background");
            _bgEffectTexture = _bgEffect.Parameters["Texture"];
            _bgEffectWvpTransform = _bgEffect.Parameters["WvpTransform"];
            _bgEffectTexTransform = _bgEffect.Parameters["TextureTransform"];

            BuildVertexBuffer();
        }

        private void CreateWvp()
        {
            var projection = Matrix.CreateOrthographicOffCenter(0, _console.GraphicsDevice.Viewport.Width,
                _console.GraphicsDevice.Viewport.Height, 0, 0, 1);
            _wvp =                 
                Matrix.CreateScale(new Vector3(
                _console.WindowArea.Width,
                _console.WindowArea.Height, 
                0))
                * Matrix.CreateTranslation(new Vector3(0, _console.WindowArea.Y, 0))
                * projection;
                
        }

        public Texture2D Texture { get; set; }
        public Vector2 TextureScale { get; set; }        
        public Matrix TextureTransform { get; set; }

        public void UnloadContent()
        {
            _bgEffect.Dispose();
        }

        public void Draw()
        {
            _bgEffectTexture.SetValue(Texture);
            _bgEffectTexTransform.SetValue(TextureTransform);
            _bgEffectWvpTransform.SetValue(_wvp);
            _bgEffect.CurrentTechnique.Passes[0].Apply();
            _console.GraphicsDevice.BlendState = BlendState.Opaque;
            _console.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            _console.GraphicsDevice.SetVertexBuffer(_vertices);
            _console.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }

        public void SetDefault(ConsoleSettings settings)
        {
            Texture = settings.BackgroundTexture;
            TextureScale = settings.BackgroundTextureScale;
            TextureTransform = settings.BackgroundTextureTransform;
        }

        private void BuildVertexBuffer()
        {
            VertexPositionTexture[] quadVertices =
            {
                new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 1))
            };
            _vertices = new VertexBuffer(_console.GraphicsDevice, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.None);
            _vertices.SetData(quadVertices);
        }
    }
}
