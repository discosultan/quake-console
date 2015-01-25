using Varus.Paradox.Console.Interpreters.Custom;

namespace Varus.Paradox.Console.Sample.Commands
{
    /// <summary>
    /// Clears the <see cref="ConsoleShell"/> input and output buffers and input history.
    /// </summary>
    public class ClearConsoleCommand : Command
    {
        private readonly ConsoleShell _consolePanel;

        /// <summary>
        /// constructs a new instance of <see cref="ClearConsoleCommand"/>.
        /// </summary>
        /// <param name="consolePanel">Console to clear.</param>
        public ClearConsoleCommand(ConsoleShell consolePanel)
        {
            _consolePanel = consolePanel;
        }

        protected override void Try(CommandResult result, string[] args)
        {            
            _consolePanel.Clear();
        }
    }
}
