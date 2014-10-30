namespace Varus.Paradox.Console.CustomInterpreter
{
    public class CommandResult
    {
        // Use this instance of CommandResult to avoid allocating garbage.
        // Only if the usage is single threaded.
        public static readonly CommandResult Default = new CommandResult();

        public bool IsFaulted { get; internal set; }
        public string Message { get; internal set; }        
    }
}
