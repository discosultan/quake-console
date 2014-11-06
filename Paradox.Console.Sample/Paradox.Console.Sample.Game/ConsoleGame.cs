using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using Varus.Paradox.Console.Sample.Utilities;

namespace Varus.Paradox.Console.Sample
{
    public class ConsoleGame : Game
    {
        private readonly ICommandInterpreter _interpreter;
        private readonly Action<Console, Cube, SpriteFont, SpriteFont> _postLoad;
        
        private Console _console;
        private readonly Cube _cube = new Cube();
        private SpriteFont _lucidaFont;
        private SpriteFont _wingdingsFont;

        private GeometricPrimitive _primitive;
        private SimpleEffect _simpleEffect;
        private Matrix _view;
        private Matrix _projection;        

        public ConsoleGame(ICommandInterpreter interpreter, Action<Console, Cube, SpriteFont, SpriteFont> postLoad)
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
            _interpreter = interpreter;
            _postLoad = postLoad;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            CreatePipeline();

            Window.AllowUserResizing = true;

            _lucidaFont = Asset.Load<SpriteFont>("Lucida");
            _wingdingsFont = Asset.Load<SpriteFont>("Wingdings");

            _console = new Console(
                Services,
                _interpreter,
                _lucidaFont) { Padding = 2 };
            GameSystems.Add(_console);

            _primitive = GeometricPrimitive.Cube.New(GraphicsDevice, 0.8f);
            // Load the texture, and create SimpleEffect
            _simpleEffect = new SimpleEffect(GraphicsDevice)
            {
                Texture = Asset.Load<Texture2D>("CubeTexture")
            };

            // Create the view and projection matrices
            _view = Matrix.LookAtRH(-Vector3.UnitZ * 10f + new Vector3(0, 2.0f, 0.0f), new Vector3(0, -4, 0), Vector3.UnitY);
            _projection = Matrix.PerspectiveFovRH((float)Math.PI / 4.0f, (float)GraphicsDevice.BackBuffer.Width / GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f);

            _postLoad(_console, _cube, _lucidaFont, _wingdingsFont);

            // Add a custom script
            Script.Add(GameScript1);
        }

        private void CreatePipeline()
        {
            // Setup the default rendering pipeline
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.CornflowerBlue });
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = Render });
        }

        private async Task GameScript1()
        {
            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                // Show garbage generation statistics.
                Garbage.Update(UpdateTime);
                Window.Title = string.Format("Garbage KB per frame {0} per second {1}", Garbage.CreatedPerFrame, Garbage.CreatedPerSecond);

                // Calculate world transformation
                var deltaSeconds = (float)DrawTime.Elapsed.TotalSeconds;
                _cube.Rotation += _cube.RotationSpeed * deltaSeconds;

                Input.KeyDown.ForEach(x => Debug.WriteLine(x));

                if (Input.IsKeyPressed(Keys.OemTilde))
                {
                    _console.ToggleOpenClose();
                }  
            }
        }

        /// <summary>
        /// Render primitive objects with SimpleEffect by supplying calculated transformation for animations
        /// </summary>
        /// <param name="renderContext"></param>
        private void Render(RenderContext renderContext)
        {
            var world =
                Matrix.Scaling(_cube.Scale) *
                Matrix.RotationYawPitchRoll(_cube.Rotation.X, _cube.Rotation.Y, _cube.Rotation.Z) *
                Matrix.Translation(_cube.Position);

            // Draw the primitive using BasicEffect
            _simpleEffect.Transform = Matrix.Multiply(world, Matrix.Multiply(_view, _projection));
            _simpleEffect.Apply();
            _primitive.Draw();
        }  
    }
}
