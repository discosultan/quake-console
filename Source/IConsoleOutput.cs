namespace QuakeConsole
{
    /// <summary>
    /// Output part of the <see cref="Console"/>. Command results will be appended here.
    /// </summary>
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
