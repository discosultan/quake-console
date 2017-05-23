namespace QuakeConsole
{
    /// <summary>
    /// A contract for the input part of the <see cref="Console"/>. Defines properties and methods
    /// required to manipulate the input.
    /// </summary>
    public interface IConsoleInput
    {
        /// <summary>
        /// Gets or sets the last autocomplete entry which was added to the buffer. Note that
        /// this value will be set to null whenever anything from the normal <see cref="Console"/>
        /// input pipeline gets appended here.
        /// </summary>
        string LastAutocompleteEntry { get; set; }

        /// <summary>
        /// Gets or sets the location of the caret. This is where user input will be appended.
        /// </summary>
        int CaretIndex { get; set; }

        /// <summary>
        /// Gets the number of characters currently in the buffer.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Gets or sets the value typed into the buffer.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Writes to the <see cref="ConsoleInput"/>.
        /// </summary>
        /// <param name="value">Message to append.</param>
        void Append(string value);

        /// <summary>
        /// Removes symbols from the <see cref="ConsoleInput"/>.
        /// </summary>
        /// <param name="startIndex">Index from which to start removing.</param>
        /// <param name="length">Number of symbols to remove.</param>
        void Remove(int startIndex, int length);

        /// <summary>
        /// Gets a substring of the buffer.
        /// </summary>
        /// <param name="startIndex">Index ta take substring from.</param>
        /// <param name="length">Number of symbols to include in the substring.</param>
        /// <returns>Substring of the buffer.</returns>
        string Substring(int startIndex, int length);

        /// <summary>
        /// Gets a substring of the buffer.
        /// </summary>
        /// <param name="startIndex">Index ta take all the following symbols from.</param>
        /// <returns>Substring of the buffer.</returns>
        string Substring(int startIndex);

        /// <summary>
        /// Clears the input from the buffer.
        /// </summary>
        void Clear();

        /// <inheritdoc/>
        string ToString();

        /// <summary>
        /// Gets the symbol at the specified index.
        /// </summary>
        /// <param name="i">Index to take symbol from.</param>
        /// <returns>Indexed symbol.</returns>
        char this[int i] { get; set; }
    }
}