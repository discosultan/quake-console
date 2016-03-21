using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace QuakeConsole
{
    internal class TexturedBackground : IDisposable
    {
        private Console _console;
        private Effect _bgEffect;
        private EffectParameter _bgEffectTexture;
        private EffectParameter _bgEffectWvpTransform;
        private EffectParameter _bgEffectTexTransform;
        private EffectParameter _bgEffectBgColor;
        private VertexBuffer _vertices;

        private Matrix _wvp;

        public void LoadContent(Console console, Effect effect)
        {
            _console = console;
            _console.WindowAreaChanged += (s, e) => CreateWvp();
            CreateWvp();
            
            _bgEffect = effect;
            _bgEffectTexture = _bgEffect.Parameters["Texture"];
            _bgEffectWvpTransform = _bgEffect.Parameters["WvpTransform"];
            _bgEffectTexTransform = _bgEffect.Parameters["TextureTransform"];
            _bgEffectBgColor = _bgEffect.Parameters["BackgroundColor"];

            BuildVertexBuffer();
        }

        public Texture2D Texture { get; set; }
        public Matrix TextureTransform { get; set; }

        public void Dispose()
        {
            _bgEffect?.Dispose();
            _vertices?.Dispose();
        }

        public void Draw()
        {            
            _bgEffectTexture.SetValue(Texture);               
            _bgEffectTexTransform.SetValue(TextureTransform);
            _bgEffectWvpTransform.SetValue(_wvp);
            _bgEffectBgColor.SetValue(_console.BackgroundColor.ToVector4());
            _bgEffect.CurrentTechnique.Passes[0].Apply();
            _console.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            _console.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            _console.GraphicsDevice.SetVertexBuffer(_vertices);
            _console.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }

        public void SetDefaults(ConsoleSettings settings)
        {
            Texture = settings.BackgroundTexture;
            TextureTransform = settings.BackgroundTextureTransform;
        }

        private Effect LoadEffectFromEmbeddedResource(string name)
        {
#if WINRT
            Assembly assembly = GetType().GetTypeInfo().Assembly;
#else
            Assembly assembly = GetType().Assembly;
#endif
            var stream = assembly.GetManifestResourceStream(name);
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);                
                return new Effect(_console.GraphicsDevice, ms.ToArray());
            }
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

        private void CreateWvp()
        {
            var projection = Matrix.CreateOrthographicOffCenter(
                0, 
                _console.GraphicsDevice.Viewport.Width,
                _console.GraphicsDevice.Viewport.Height, 
                0, 0, 1);
            _wvp = Matrix.CreateScale(new Vector3(_console.WindowArea.Width, _console.WindowArea.Height, 0))*
                   Matrix.CreateTranslation(new Vector3(0, _console.WindowArea.Y, 0))*
                   projection;

        }        
    }
}
