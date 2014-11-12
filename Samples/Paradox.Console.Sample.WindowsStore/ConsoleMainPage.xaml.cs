// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

using System;
using Windows.UI.Xaml.Controls;
using SiliconStudio.Paradox.Graphics;
using Varus.Paradox.Console.CustomInterpreter;
using Varus.Paradox.Console.Sample.Commands;

namespace Varus.Paradox.Console.Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConsoleMainPage : Page
    {
        public ConsoleMainPage()
        {
            InitializeComponent();

            var customInterpreter = new CustomCommandInterpreter();
            Action<Console, Cube, SpriteFont, SpriteFont> postLoad = (console, cube, font1, font2) =>
            {
                customInterpreter.RegisterCommand("Clear-Console", new ClearConsoleCommand(console));
                customInterpreter.RegisterCommand("Set-Cube-Rotation-Speed", new SetCubeRotationSpeedCommand(cube));
                customInterpreter.RegisterCommand("Set-Cube-Position", new SetCubePositionCommand(cube));
            };

            var game = new ConsoleGame(customInterpreter, postLoad);
            game.Run(SwapChainPanel);
        }
    }
}
