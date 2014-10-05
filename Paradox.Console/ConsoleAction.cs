namespace Varus.Paradox.Console
{
    public enum ConsoleAction : byte
    {        
        None,
        DeletePreviousChar,
        AutoComplete,
        ExecuteCommand,
        UppercaseModifier,
        SpecialModifier,
        UppercaseToggle,
        Space,        
        GoToEnd,
        GoToBeginning,
        MoveLeft,
        PreviousCommandInHistory,
        MoveRight,
        NextCommandInHistory,
        //Insert, // input modifier
        DeleteCurrentChar,
        //NumLock,      
        Clear,        
        Copy
    }
}
