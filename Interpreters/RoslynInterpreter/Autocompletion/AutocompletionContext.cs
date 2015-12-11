namespace QuakeConsole
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
            return $"{Context} StartIndex: {StartIndex}";
        }
    }

    internal enum AutocompletionType
    {
        Regular,
        Accessor,
        Assignment
    }
}
