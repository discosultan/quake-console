using Varus.Paradox.Console.CustomInterpreter;

namespace Varus.Paradox.Console.Sample.Commands
{
    /// <summary>
    /// Clears the <see cref="ConsolePanel"/> input and output buffers and input history.
    /// </summary>
    public class ClearConsoleCommand : Command
    {
        private readonly ConsolePanel _consolePanel;

        /// <summary>
        /// constructs a new instance of <see cref="ClearConsoleCommand"/>.
        /// </summary>
        /// <param name="consolePanel">Console to clear.</param>
        public ClearConsoleCommand(ConsolePanel consolePanel)
        {
            _consolePanel = consolePanel;
        }

        protected override void Try(CommandResult result, string[] args)
        {            
            _consolePanel.Clear();
        }
    }
}
