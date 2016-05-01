using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace QuakeConsole
{
    /// <summary>
    /// Command-line interface with swappable command interpreters.
    /// </summary>
    public class ConsoleComponent : DrawableGameComponent
    {
        private readonly Console _console = new Console();
        private ContentManager _content;

        private bool _initialized;

        /// <inheritdoc />        
        public ConsoleComponent(Game game) : base(game)
        { }

        /// <summary>
        /// Gets if the console is currently reading keyboard input.
        /// </summary>
        public bool IsAcceptingInput => _console.IsAcceptingInput;

        /// <summary>
        /// Gets if any part of the <see cref="Console"/> is visible.
        /// </summary>
        public bool IsVisible => _console.IsVisible;

        /// <summary>
        /// Gets or sets the command interpreter. This defines how user input commands
        /// are evaluated and operated on. Optionally provides autocompletion.
        /// Pass NULL to use a stub interpreter instead (useful for testing just the shell).
        /// </summary>
        public ICommandInterpreter Interpreter
        {
            get { return _console.Interpreter; }
            set { _console.Interpreter = value; }
        }

        /// <summary>
        /// Gets or sets the input command logging delegate. Set this property to log the user input
        /// commands to the given delegate. For example WriteLine(String).
        /// </summary>
        public Action<string> LogInput
        {
            get { return _console.ConsoleInput.CommandExecution.LogInput; }
            set { _console.ConsoleInput.CommandExecution.LogInput = value; }
        }

        /// <summary>
        /// Gets or sets the dictionary that is used to map keyboard keys to corresponding symbols
        /// shown in the <see cref="Console"/>.
        /// </summary>
        public Dictionary<Keys, Symbol> CharacterMappings
        {
            get { return _console.ConsoleInput.SymbolMappings; }
            set { _console.ConsoleInput.SymbolMappings = value; }
        }

        /// <summary>
        /// Gets the dictionary that is used to map keyboard keys to corresponding console
        /// actions (such as ExecuteCommand or ToggleUppercase).
        /// </summary>
        public ConsoleActionMap ActionMappings => _console.ConsoleInput.ActionDefinitions;

        /// <summary>
        /// Gets or sets the font used to render console text.
        /// </summary>
        public SpriteFont Font
        {
            get { return _console.Font; }
            set { _console.Font = value; }
        }

        /// <summary>
        /// Gets or sets the padding to apply to the borders of the <see cref="Console"/> window.
        /// Note that padding will be automatically decreased if the available window area becomes too low.
        /// </summary>
        public float Padding
        {
            get { return _console.Padding; }
            set { _console.Padding = value; }
        }

        /// <summary>
        /// Gets or sets the background color. Supports transparency.
        /// </summary>
        public Color BackgroundColor
        {
            get {  return _console.BackgroundColor; }  
            set { _console.BackgroundColor = value; }
        }

        /// <summary>
        /// Gets or sets the font color. Supports transparency.
        /// </summary>
        public Color FontColor
        {
            get { return _console.FontColor; }
            set { _console.FontColor = value; }
        }

        /// <summary>
        /// Gets or sets the time in seconds it takes to fully open or close the <see cref="Console"/>.
        /// </summary>
        public float TimeToToggleOpenClose
        {
            get { return _console.OpenCloseTransitionSeconds; }
            set { _console.OpenCloseTransitionSeconds = value; }
        }

        /// <summary>
        /// Gets or sets the time in seconds it takes to trigger repeating input feature.
        /// Repeating input is triggered when a key is held down. When repeating input is triggered, 
        /// keys which are held down will be repeatedly processed or appended to the console 
        /// (normally they are processed or appended only upon key press).
        /// </summary>
        public float TimeToTriggerRepeatingInput
        {
            get { return _console.ConsoleInput.RepeatingInput.TimeUntilRepeatingInput; }
            set { _console.ConsoleInput.RepeatingInput.TimeUntilRepeatingInput = value; }
        }

        /// <summary>
        /// Gets or sets the time in seconds it takes before another character is appended when
        /// repeating input is enabled. See TimeToTriggerRepeatingInput for more information.
        /// </summary>
        public float TimeToCooldownRepeatingInput
        {
            get { return _console.ConsoleInput.RepeatingInput.RepeatingInputCooldown; }
            set { _console.ConsoleInput.RepeatingInput.RepeatingInputCooldown = value; }
        }

        /// <summary>
        /// Gets or sets the number of symbols that will be brought into console input view once the user moves
        /// the caret out of the visible area.
        /// </summary>
        public int NumSymbolsToMoveWhenCaretOutOfView
        {
            get { return _console.ConsoleInput.NumPositionsToMoveWhenOutOfScreen; }
            set { _console.ConsoleInput.NumPositionsToMoveWhenOutOfScreen = value; }
        }

        /// <summary>
        /// Gets or sets the percentage of height the <see cref="Console"/> window takes in relation to
        /// application window height. Value in between [0...1].
        /// </summary>
        public float HeightRatio
        {
            get { return _console.HeightRatio; }
            set { _console.HeightRatio = value; }
        }

        /// <summary>
        /// Gets or sets the textual symbol(s) that is shown in the beginning of console input line.
        /// </summary>
        public string InputPrefix
        {
            get { return _console.ConsoleInput.InputPrefix; }
            set { _console.ConsoleInput.InputPrefix = value; }
        }

        /// <summary>
        /// Gets or sets the color for the input prefix symbol. See InputPrefix for more information.
        /// </summary>
        public Color InputPrefixColor
        {
            get { return _console.ConsoleInput.InputPrefixColor; }
            set { _console.ConsoleInput.InputPrefixColor = value; }
        }

        /// <summary>
        /// Gets or sets the symbol which is used as the caret. This symbol is used to indicate
        /// where the user input will be appended.
        /// </summary>
        public string CaretSymbol
        {
            get { return _console.ConsoleInput.Caret.Symbol; }
            set { _console.ConsoleInput.Caret.Symbol = value; }
        }

        /// <summary>
        /// Gets or sets the time in seconds to toggle caret's visibility.
        /// </summary>
        public float CaretBlinkingInterval
        {
            get { return _console.ConsoleInput.Caret.BlinkIntervalSeconds; }
            set { _console.ConsoleInput.Caret.BlinkIntervalSeconds = value; }
        }

        /// <summary>
        /// Gets or sets if rows which run out of the visible area of the console output window should be removed.
        /// </summary>
        public bool RemoveOverflownEntries
        {
            get { return _console.ConsoleOutput.RemoveOverflownEntries; }
            set { _console.ConsoleOutput.RemoveOverflownEntries = value; }
        }

        /// <summary>
        /// Gets or sets the color of the border at the bottom of the console window.
        /// Supports transparency.
        /// </summary>
        public Color BottomBorderColor
        {
            get { return _console.BottomBorderColor; }
            set { _console.BottomBorderColor = value; }
        }

        /// <summary>
        /// Gets or sets the thickness of the border at the bottom of the console window in pixels.
        /// To disable border, set this value less than or equal to zero.
        /// </summary>
        public float BottomBorderThickness
        {
            get { return _console.BottomBorderThickness; }
            set { _console.BottomBorderThickness = value; }
        }

        /// <summary>
        /// Gets or sets the texture used to render as the console background. Set this to NULL
        /// to disable textured background.
        /// </summary>
        public Texture2D BackgroundTexture
        {
            get { return _console.BgRenderer.Texture; }
            set { _console.BgRenderer.Texture = value; }
        }

        /// <summary>
        /// Gets or sets the transformation applied to texture coordinates if background texture is set.
        /// </summary>
        public Matrix BackgroundTextureTransform
        {
            get { return _console.BgRenderer.TextureTransform; }
            set { _console.BgRenderer.TextureTransform = value; }
        }

        /// <summary>
        /// Gets or sets the symbol used to represent a tab.
        /// </summary>
        /// <remarks>
        /// By default, four spaces are used to simulate a tab since a lot of
        /// <see cref="SpriteFont"/>s don't support the \t char.
        /// </remarks>
        public string TabSymbol
        {
            get { return _console.TabSymbol; }
            set { _console.TabSymbol = value; }
        }

        /// <summary>
        /// Gets or sets the color used to draw the background of the selected portion of user input.
        /// </summary>
        public Color SelectionColor
        {
            get { return _console.ConsoleInput.Selection.Color; }
            set { _console.ConsoleInput.Selection.Color = value; }
            
        }

        /// <summary>
        /// Opens the console windows if it is closed. Closes it if it is opened.
        /// </summary>
        public void ToggleOpenClose() => _console.ToggleOpenClose();        

        /// <summary>
        /// Clears the subparts of the <see cref="Console"/>.
        /// </summary>
        /// <param name="clearFlags">Specifies which subparts to clear.</param>
        public void Clear(ConsoleClearFlags clearFlags = ConsoleClearFlags.All) => _console.Clear(clearFlags);

        /// <summary>
        /// Clears the <see cref="Console"/> and sets all the settings
        /// to their default values.
        /// </summary>
        public void Reset() => _console.Reset();

        /// <summary>
        /// Gets the input writer of the console.
        /// </summary>
        public IConsoleInput Input => _console.ConsoleInput;

        /// <summary>
        /// Gets the output writer of the console.
        /// </summary>
        public IConsoleOutput Output => _console.ConsoleOutput;

        /// <inheritdoc/>
        public override void Initialize()
        {
            if (_initialized) return;
            _content = new ResourceContentManager(
                Game.Services,
#if WINDOWSDX
                Resource_WindowsDX.ResourceManager
#else
#endif
            );
            _console.LoadContent(
                GraphicsDevice,
                (GraphicsDeviceManager)Game.Services.GetService<IGraphicsDeviceManager>(),
                _content.Load<SpriteFont>("qc_console"),
                _content.Load<Effect>("qc_background"));
            _initialized = true;
        }

        /// <inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            if (Enabled)
            {
                EnsureInitialized();
                _console.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime)
        {
            if (Visible)
            {
                EnsureInitialized();
                _console.Draw();
            }
        }

        /// <inheritdoc/>
        protected override void UnloadContent()
        {            
            _console.Dispose();
            _content?.Dispose();
        }

        /// <inheritdoc />        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                UnloadContent();            
        }

        private void EnsureInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException($"{nameof(ConsoleComponent)} must be initialized first!");
        }
    }
}