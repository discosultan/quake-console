namespace Varus.Paradox.Console
{
    /// <summary>
    /// Provides a stub command interpreter which does nothing.
    /// </summary>
    public class StubCommandInterpreter : ICommandInterpreter
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="outputBuffer">Console output buffer to append any output messages.</param>
        /// <param name="command">Command to execute.</param>
        public void Execute(OutputBuffer outputBuffer, string command)
        {
            outputBuffer.Append(command);
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="inputBuffer">Console input.</param>
        /// <param name="isNextValue">True if user wants to autocomplete to the next value; false if to the previous value.</param>
        public void Autocomplete(InputBuffer inputBuffer, bool isNextValue)
        {            
        }
    }
}
