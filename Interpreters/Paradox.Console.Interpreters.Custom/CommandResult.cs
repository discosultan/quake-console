namespace Varus.Paradox.Console.Interpreters.Custom
{
    /// <summary>
    /// Result of command execution process.
    /// </summary>
    public class CommandResult
    {
        // Use this instance of CommandResult to avoid allocating garbage.
        // Only if the usage is single threaded.
        public static readonly CommandResult Default = new CommandResult();

        /// <summary>
        /// Gets or sets if the command execution was faulted.
        /// </summary>
        public bool IsFaulted { get; set; }
        /// <summary>
        /// Gets or sets the message associated with the command execution.
        /// </summary>
        public string Message { get; set; }        
    }
}
