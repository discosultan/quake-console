namespace QuakeConsole
{
    /// <summary>
    /// A contract for a <see cref="Console"/> command interpreter. Manages command execution and autocompletion features.
    /// </summary>
    public interface ICommandInterpreter
    {
        /// <summary>
        /// Executes a <see cref="Console"/> command.
        /// </summary>
        /// <param name="outputBuffer">Buffer to append data which is shown to the user.</param>
        /// <param name="command">Command to execute.</param>
        void Execute(IOutputBuffer outputBuffer, string command);
        /// <summary>
        /// Tries to autocomplete the current user input in the <see cref="InputBuffer"/>.
        /// </summary>
        /// <param name="inputBuffer">Buffer to read from and autocomplete user input.</param>
        /// <param name="forward">Indicator which indicates whether we should move forward or backward with the autocomplete entries.</param>
        void Autocomplete(IInputBuffer inputBuffer, bool forward);
    }

    /// <summary>
    /// Provides a stub command interpreter which does nothing.
    /// </summary>
    internal class StubCommandInterpreter : ICommandInterpreter
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="outputBuffer">Console output buffer to append any output messages.</param>
        /// <param name="command">Command to execute.</param>
        public void Execute(IOutputBuffer outputBuffer, string command)
        {
            outputBuffer.Append(command);
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="inputBuffer">Console input.</param>
        /// <param name="forward">True if user wants to autocomplete to the next value; false if to the previous value.</param>
        public void Autocomplete(IInputBuffer inputBuffer, bool forward)
        {
        }
    }
}
