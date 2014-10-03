using SiliconStudio.Core;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;

namespace Varus.Paradox.Console
{
    public class Console : GameSystemBase
    {        
        private readonly IConsoleInterpreter _interpreter;
        private readonly IServiceRegistry _registry;

        private Texture2D _backgroundTexture;

        public Console(IServiceRegistry registry, IConsoleInterpreter interpreter)
            : base(registry)
        {            
            _registry = registry;
            _interpreter = interpreter;            
        }

        protected override void LoadContent()
        {            
            _backgroundTexture = Texture2D.New(GraphicsDevice, 1, 1, PixelFormat.R32G32B32A32_Float);            
        }

        public override void Update(GameTime gameTime)
        {            
        }

        public override void Draw(GameTime gameTime)
        {
        }

        protected override void UnloadContent()
        {
            _backgroundTexture.Dispose();
        }
    }
}
