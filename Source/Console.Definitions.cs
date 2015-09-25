using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using QuakeConsole.Utilities;
#if MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endif

namespace QuakeConsole
{
    internal partial class Console
    {
        private readonly BiDirectionalDictionary<Keys, ConsoleAction> _actionDefinitions = new BiDirectionalDictionary<Keys, ConsoleAction>
        {
#if MONOGAME
            { Keys.LeftControl, ConsoleAction.CopyPasteModifier },
            { Keys.RightControl, ConsoleAction.CopyPasteModifier },
            { Keys.LeftControl, ConsoleAction.AutocompleteModifier },
            { Keys.RightControl, ConsoleAction.AutocompleteModifier },
#else
            { Keys.NumPadEnter, ConsoleAction.ExecuteCommand },
            { Keys.LeftCtrl, ConsoleAction.CopyPasteModifier },
            { Keys.RightCtrl, ConsoleAction.CopyPasteModifier },
            { Keys.LeftCtrl, ConsoleAction.AutocompleteModifier },
            { Keys.RightCtrl, ConsoleAction.AutocompleteModifier },
#endif
            { Keys.Enter, ConsoleAction.ExecuteCommand },
            { Keys.Left, ConsoleAction.MoveLeft },
            { Keys.Right, ConsoleAction.MoveRight },
            { Keys.Home, ConsoleAction.MoveToBeginning },
            { Keys.End, ConsoleAction.MoveToEnd },
            { Keys.Back, ConsoleAction.DeletePreviousChar },
            { Keys.Delete, ConsoleAction.DeleteCurrentChar },
            { Keys.LeftShift, ConsoleAction.UppercaseModifier },
            { Keys.RightShift, ConsoleAction.UppercaseModifier },
            { Keys.Up, ConsoleAction.PreviousCommandInHistory },
            { Keys.Down, ConsoleAction.NextCommandInHistory },                                    
            { Keys.LeftShift, ConsoleAction.PreviousEntryModifier },
            { Keys.RightShift, ConsoleAction.PreviousEntryModifier },
            { Keys.V, ConsoleAction.Paste },
            { Keys.LeftShift, ConsoleAction.NextLineModifier },
            //{ Keys.Tab, ConsoleAction.Autocomplete },            
            { Keys.Space, ConsoleAction.Autocomplete },            
            { Keys.Tab, ConsoleAction.Tab },
            { Keys.LeftShift, ConsoleAction.TabModifier },
            { Keys.RightShift, ConsoleAction.TabModifier }
        };

        private Dictionary<Keys, SymbolPair> _symbolDefinitions = new Dictionary<Keys, SymbolPair>
        {
            // Digits.
            { Keys.D1, new SymbolPair("1", "!") },
            { Keys.D2, new SymbolPair("2", "@") },
            { Keys.D3, new SymbolPair("3", "#") },
            { Keys.D4, new SymbolPair("4", "$") },
            { Keys.D5, new SymbolPair("5", "%") },
            { Keys.D6, new SymbolPair("6", "^") },
            { Keys.D7, new SymbolPair("7", "&") },
            { Keys.D8, new SymbolPair("8", "*") },
            { Keys.D9, new SymbolPair("9", "(") },
            { Keys.D0, new SymbolPair("0", ")") },
            { Keys.NumPad1, new SymbolPair("1") },
            { Keys.NumPad2, new SymbolPair("2") },
            { Keys.NumPad3, new SymbolPair("3") },
            { Keys.NumPad4, new SymbolPair("4") },
            { Keys.NumPad5, new SymbolPair("5") },
            { Keys.NumPad6, new SymbolPair("6") },
            { Keys.NumPad7, new SymbolPair("7") },
            { Keys.NumPad8, new SymbolPair("8") },
            { Keys.NumPad9, new SymbolPair("9") },
            { Keys.NumPad0, new SymbolPair("0") },

            // Letters.
            { Keys.Q, new SymbolPair("q", "Q") },
            { Keys.W, new SymbolPair("w", "W") },
            { Keys.E, new SymbolPair("e", "E") },
            { Keys.R, new SymbolPair("r", "R") },
            { Keys.T, new SymbolPair("t", "T") },
            { Keys.Y, new SymbolPair("y", "Y") },
            { Keys.U, new SymbolPair("u", "U") },
            { Keys.I, new SymbolPair("i", "I") },
            { Keys.O, new SymbolPair("o", "O") },
            { Keys.P, new SymbolPair("p", "P") },
            { Keys.OemOpenBrackets, new SymbolPair("[", "{") },
            { Keys.OemCloseBrackets, new SymbolPair("]", "}") },

            { Keys.A, new SymbolPair("a", "A") },
            { Keys.S, new SymbolPair("s", "S") },
            { Keys.D, new SymbolPair("d", "D") },
            { Keys.F, new SymbolPair("f", "F") },
            { Keys.G, new SymbolPair("g", "G") },
            { Keys.H, new SymbolPair("h", "H") },
            { Keys.J, new SymbolPair("j", "J") },
            { Keys.K, new SymbolPair("k", "K") },
            { Keys.L, new SymbolPair("l", "L") },
            { Keys.OemSemicolon, new SymbolPair(";", ":") },
            { Keys.OemQuotes, new SymbolPair("'", "\"") },
#if PARADOX
            { Keys.Oem5, new SymbolPair("\\", "|") },            
#endif

            { Keys.OemBackslash, new SymbolPair("\\", "|") },
            { Keys.Z, new SymbolPair("z", "Z") },
            { Keys.X, new SymbolPair("x", "X") },
            { Keys.C, new SymbolPair("c", "C") },
            { Keys.V, new SymbolPair("v", "V") },
            { Keys.B, new SymbolPair("b", "B") },
            { Keys.N, new SymbolPair("n", "N") },
            { Keys.M, new SymbolPair("m", "M") },
            { Keys.OemComma, new SymbolPair(",", "<") },
            { Keys.OemPeriod, new SymbolPair(".", ">") },
#if PARADOX
            { Keys.Oem2, new SymbolPair("/", "?") },
#endif

            // Special.
            { Keys.Space, new SymbolPair(" ", " ") },
            { Keys.OemMinus, new SymbolPair("-", "_") },
            { Keys.OemPlus, new SymbolPair("=", "+") },
            { Keys.Decimal, new SymbolPair(".") },
            { Keys.Add, new SymbolPair("+") },
            { Keys.Subtract, new SymbolPair("-") },
            { Keys.Multiply, new SymbolPair("*") },
            { Keys.Divide, new SymbolPair("/") },
            //{ Keys.Tab, new SymbolPair("\t", "\t") } // Tab char is not supported in many fonts.
            //{ Keys.Tab, new SymbolPair("    ", "    ") } // Use 4 spaces instead.
        };

        internal readonly string NewLine = "\n";
        internal readonly string Tab = "    ";
    }

    /// <summary>
    /// Defines which subparts of the <see cref="Console"/> to clear.
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

    internal enum ConsoleProcessResult
    {
        None,
        Continue,
        Break
    }

    internal enum ConsoleState
    {
        Closed,
        Closing,
        Open,
        Opening
    }

    internal class ConsoleSettings
    {
        public Color BackgroundColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        public Color FontColor { get; set; } = new Color(1.0f, 1.0f, 0.0f, 1.0f);
        public float TimeToToggleOpenClose { get; set; } = 0.25f;
        public float TimeToTriggerRepeatingInput { get; set; } = 0.4f;
        public float TimeToCooldownRepeatingInput { get; set; } = 0.04f;
        public float HeightRatio { get; set; } = 0.4f;
        public string InputPrefix { get; set; } = "]";
        public int NumPositionsToMoveWhenOutOfScreen { get; set; } = 4;
        public Color InputPrefixColor { get; set; } = Color.Yellow;
        public float Padding { get; set; } = 2.0f;
        public string CaretSymbol { get; set; } = "_";
        public float CaretBlinkingIntervalSeconds { get; set; } = 0.4f;
        public bool BottomBorderEnabled { get; set; }
        public Color BottomBorderColor { get; set; } = Color.Red;
        public float BottomBorderThickness { get; set; } = 5.0f;
        public Texture2D BackgroundTexture { get; set; }
        public Vector2 BackgroundTextureScale { get; set; } = Vector2.One;
        public Matrix BackgroundTextureTransform { get; set; } = Matrix.Identity;
    }
}
