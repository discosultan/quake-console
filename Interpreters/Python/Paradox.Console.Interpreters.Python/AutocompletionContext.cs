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

        public override string ToString()
        {
            return string.Format("{0} StartIndex: {1}", Context, StartIndex);
        }
    }
}
