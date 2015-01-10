namespace Varus.Paradox.Console
{
    /// <summary>
    /// A contract for a <see cref="ConsoleShell"/> command interpreter. Manages command execution and autocompletion features.
    /// </summary>
    public interface ICommandInterpreter
    {
        /// <summary>
        /// Executes a <see cref="ConsoleShell"/> command.
        /// </summary>
        /// <param name="outputBuffer">Buffer to append data which is shown to the user.</param>
        /// <param name="command">Command to execute.</param>
        void Execute(OutputBuffer outputBuffer, string command);
        /// <summary>
        /// Tries to autocomplete the current user input in the <see cref="InputBuffer"/>.
        /// </summary>
        /// <param name="inputBuffer">Buffer to read from and autocomplete user input.</param>
        /// <param name="isNextValue">Indicator which indicates whether we should move forward or backward with the autocomplete entries.</param>
        void Autocomplete(InputBuffer inputBuffer, bool isNextValue);
    }
}
