using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;

namespace Varus.Paradox.Console.Testbed
{
    public class WindowsGame : Game
    {
        private Varus.Paradox.Console.Console console;

        public WindowsGame()
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            CreatePipeline();

            console = new Varus.Paradox.Console.Console(
                Services, 
                new StubConsoleCommandInterpreter(),                
                Asset.Load<SpriteFont>("SpriteFont"));
            GameSystems.Add(console);
            // Add custom game init at load time here
            // ...

            // Add a custom script
            Script.Add(GameScript1);
        }

        private void CreatePipeline()
        {
            // Setup the default rendering pipeline
            RenderSystem.Pipeline.Renderers.Add(new CameraSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.CornflowerBlue });
            RenderSystem.Pipeline.Renderers.Add(new ModelRenderer(Services, "WindowsEffectMain"));
            RenderSystem.Pipeline.Renderers.Add(new UIRenderer(Services));
        }

        private async Task GameScript1()
        {
            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                if (Input.IsKeyPressed(Keys.OemTilde))
                {
                    console.ToggleOpenClose();
                }

                // Add custom code to run every frame here (move entity...etc.)
                // ...
            }
        }
    }
}
