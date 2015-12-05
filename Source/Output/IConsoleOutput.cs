namespace QuakeConsole
{
    /// <summary>
    /// A contract for the output part of the <see cref="Console"/>. Defines methods manipulating
    /// the output.
    /// </summary>
    /// <remarks>Used, for example, to clear the output window or append results from outside the console.</remarks>
    public interface IConsoleOutput
    {
        /// <summary>
        /// Appends a message to the buffer.
        /// </summary>
        /// <param name="message">Message to append.</param>
        void Append(string message);

        /// <summary>
        /// Clears all the information in the buffer.
        /// </summary>
        void Clear();
    }
}
