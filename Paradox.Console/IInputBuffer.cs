namespace Varus.Paradox.Console
{
    public interface IInputBuffer
    {
        /// <summary>
        /// Gets or sets the last autocomplete entry which was added to the buffer. Note that
        /// this value will be set to null whenever anything from the normal <see cref="ConsoleShell"/>
        /// input pipeline gets appended here.
        /// </summary>
        string LastAutocompleteEntry { get; set; }

        /// <summary>
        /// Gets the <see cref="Caret"/> associated with the buffer. This indicates where user input will be appended.
        /// </summary>
        ICaret Caret { get; }

        /// <summary>
        /// Gets the number of characters currently in the buffer.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Gets or sets the value typed into the buffer.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Writes symbol to the <see cref="InputBuffer"/>.
        /// </summary>
        /// <param name="symbol">Symbol to write.</param>
        void Write(string symbol);

        /// <summary>
        /// Removes symbols from the <see cref="InputBuffer"/>.
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
        char this[int i] { get; }
    }
}