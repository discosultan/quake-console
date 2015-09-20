namespace QuakeConsole
{
    /// <summary>
    /// Represents a pair of lowercase and uppercase symbols.
    /// </summary>
    public class SymbolPair
    {
        /// <summary>
        /// Gets the lowercase symbol.
        /// </summary>
        public string LowercaseSymbol { get; private set; }
        /// <summary>
        /// Gets the uppercase symbol.
        /// </summary>
        public string UppercaseSymbol { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="SymbolPair"/>.
        /// </summary>
        /// <param name="lowercase">Lowercase symbol of the pair.</param>
        /// <param name="uppercase">Uppercase symbol of the pair.</param>
        public SymbolPair(string lowercase, string uppercase = null)
        {
            LowercaseSymbol = lowercase;
            UppercaseSymbol = uppercase;
        }
    }
}
