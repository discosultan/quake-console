using Varus.Paradox.Console.CustomInterpreter;

namespace Varus.Paradox.Console.Sample.Commands
{
    /// <summary>
    /// Clears the <see cref="Console"/> input and output buffers and input history.
    /// </summary>
    public class ClearConsoleCommand : Command
    {
        private readonly Console _console;

        /// <summary>
        /// constructs a new instance of <see cref="ClearConsoleCommand"/>.
        /// </summary>
        /// <param name="console">Console to clear.</param>
        public ClearConsoleCommand(Console console)
        {
            _console = console;
        }

        protected override void Try(CommandResult result, string[] args)
        {            
            _console.Clear();
        }
    }
}
