namespace Varus.Paradox.Console
{
    public class SymbolPair
    {
        public string LowercaseSymbol { get; set; }
        public string UppercaseSymbol { get; set; }

        public SymbolPair(string lowercase, string uppercase)
        {
            LowercaseSymbol = lowercase;
            UppercaseSymbol = uppercase;
        }
    }
}
