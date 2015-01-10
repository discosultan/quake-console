using System;

namespace Varus.Paradox.Console
{
    /// <summary>
    /// Defines which subparts of the <see cref="ConsolePanel"/> to clear.
    /// </summary>
    [Flags]
    public enum ConsoleClearFlags
    {
        None = 0,
        OutputBuffer = 1,
        InputBuffer = 2,
        InputHistory = 4,        
        All = OutputBuffer | InputBuffer | InputHistory
    }
}
