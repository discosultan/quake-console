namespace QuakeConsole
{
    internal enum ConsoleAction : byte
    {        
        None,
        DeletePreviousChar,
        Autocomplete,
        ExecuteCommand,
        NextLineModifier,        
        UppercaseModifier,
        CopyPasteModifier,
        PreviousEntryModifier,
        AutocompleteModifier,
        //UppercaseToggle,
        Space,        
        MoveToEnd,
        MoveToBeginning,
        MoveLeft,
        PreviousCommandInHistory,
        MoveRight,
        NextCommandInHistory,
        //Insert, // input modifier
        DeleteCurrentChar,
        //NumLock,      
        Clear,        
        Copy,
        Paste,
        Tab,
        TabModifier
    }
}
