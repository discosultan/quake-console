namespace Varus.Paradox.Console.Interpreters.Python
{
    internal enum AutocompletionContext
    {
        Regular,
        Method
    }

    internal struct AutocompletionContextResult
    {
        public AutocompletionContext Context;
        public int StartIndex;
    }
}
