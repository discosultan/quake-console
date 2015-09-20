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
        private const Keys ToggleOpenCloseKey = Keys.OemTilde;

        private readonly ICommandInterpreter _interpreter;
        private readonly Action<ConsoleShell, Cube, SpriteFont, SpriteFont> _postLoad;
        
        private ConsoleShell _console;
        private readonly Cube _cube = new Cube();
        private SpriteFont _lucidaFont;
        private SpriteFont _wingdingsFont;

        private GeometricPrimitive _primitive;
        private SimpleEffect _simpleEffect;
        private Matrix _view;
        private Matrix _projection;
        private SpriteBatch _spriteBatch;

        public ConsoleGame(ICommandInterpreter interpreter, Action<ConsoleShell, Cube, SpriteFont, SpriteFont> postLoad)
        {                        
            _interpreter = interpreter;
            _postLoad = postLoad;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            CreatePipeline();

            // Allow use to resize the game window.
            Window.AllowUserResizing = true;

            // Load fonts.
            _lucidaFont = Asset.Load<SpriteFont>("Lucida");
            _wingdingsFont = Asset.Load<SpriteFont>("Wingdings");            

            // Create console and add it to game systems.
            _console = new ConsoleShell(Services, _lucidaFont, _interpreter)
            {
                Padding = 2
            };
            GameSystems.Add(_console);

            // Log input commands in DEBUG build.
            _console.InputLog = cmd => Debug.WriteLine(cmd);

            // Create cube and load the effect to render it.
            _primitive = GeometricPrimitive.Cube.New(GraphicsDevice, 0.8f);            
            _simpleEffect = new SimpleEffect(GraphicsDevice)
            {
                Texture = Asset.Load<Texture>("CubeTexture")
            };

            // Create the view and projection matrices.
            _view = Matrix.LookAtRH(-Vector3.UnitZ * 10f + new Vector3(0, 2.0f, 0.0f), new Vector3(0, -4, 0), Vector3.UnitY);
            _projection = Matrix.PerspectiveFovRH((float)Math.PI / 4.0f, (float)GraphicsDevice.BackBuffer.Width / GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f);

            // Call post-load delegate to allow platform specific code to register stuff with the interpreter.
            _postLoad(_console, _cube, _lucidaFont, _wingdingsFont);

            // Create SpriteBatch for drawing instruction texts.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Add a custom script.
            Script.Add(GameScript1);
        }

        private void CreatePipeline()
        {
            // Setup the default rendering pipeline.
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.CornflowerBlue });
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));            
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = Render });
        }

        private async Task GameScript1()
        {
            while (IsRunning)
            {
                // Wait next rendering frame.
                await Script.NextFrame();

                // Calculate world transformation.
                var deltaSeconds = (float)DrawTime.Elapsed.TotalSeconds;
                _cube.Rotation += _cube.RotationSpeed * deltaSeconds;

                // Check if console state change was toggled.
                if (Input.IsKeyPressed(ToggleOpenCloseKey))
                {
                    _console.ToggleOpenClose();
                } 

                // Show garbage generation statistics.
                Garbage.Update(UpdateTime);
                Window.Title = string.Format("Garbage KB per frame {0} per second {1}", Garbage.CreatedPerFrame, Garbage.CreatedPerSecond);                

                // Output currently pressed keys.
                Input.KeyEvents.ForEach(x => { if (x.Type == KeyEventType.Pressed) Debug.WriteLine(x); });                
            }
        }

        /// <summary>
        /// Render primitive objects with SimpleEffect by supplying calculated transformation for animations.
        /// </summary>
        /// <param name="renderContext"></param>
        private void Render(RenderContext renderContext)
        {
            var world =
                Matrix.Scaling(_cube.Scale) *
                Matrix.RotationYawPitchRoll(_cube.Rotation.X, _cube.Rotation.Y, _cube.Rotation.Z) *
                Matrix.Translation(_cube.Position);

            // Draw the primitive using BasicEffect.
            _simpleEffect.Transform = Matrix.Multiply(world, Matrix.Multiply(_view, _projection));
            _simpleEffect.Apply();
            _primitive.Draw();

            // Draw instructions.
            const float padding = 10f;
            string msg = string.Format(
                "Press {0} to toggle console panel.\nPress {1} to autocomplete input values.\nPress {2} to navigate through input history.", 
                ToggleOpenCloseKey,
                Keys.LeftCtrl + " + " + Keys.Space,
                Keys.Up + " or " + Keys.Down);
            _spriteBatch.Begin();
            _spriteBatch.DrawString(
                _lucidaFont,
                msg,
                new Vector2(padding, GraphicsDevice.BackBuffer.Height - _lucidaFont.MeasureString(msg).Y - padding),
                Color.Yellow);
            _spriteBatch.End();
        }  
    }
}
