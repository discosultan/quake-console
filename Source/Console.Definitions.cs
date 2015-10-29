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
        internal ConsoleActionMap ActionDefinitions { get; } = new ConsoleActionMap
        {
#if PARADOX
            { Keys.NumPadEnter, ConsoleAction.ExecuteCommand },
            { Keys.LeftShift, Keys.NumPadEnter, ConsoleAction.NextLineModifier },
            { Keys.RightShift, Keys.NumPadEnter, ConsoleAction.NextLineModifier },
#endif            
            { Keys.Enter, ConsoleAction.ExecuteCommand },
            { Keys.Left, ConsoleAction.MoveLeft },
            { Keys.Right, ConsoleAction.MoveRight },
            { Keys.LeftControl, Keys.Left, ConsoleAction.MoveLeftWord },
            { Keys.RightControl, Keys.Left, ConsoleAction.MoveLeftWord },
            { Keys.LeftControl, Keys.Right, ConsoleAction.MoveRightWord },
            { Keys.RightControl, Keys.Right, ConsoleAction.MoveRightWord },
            { Keys.Home, ConsoleAction.MoveToBeginning },
            { Keys.End, ConsoleAction.MoveToEnd },
            { Keys.Back, ConsoleAction.DeletePreviousChar },
            { Keys.Delete, ConsoleAction.DeleteCurrentChar },
            { Keys.LeftShift, ConsoleAction.UppercaseModifier }, // ?
            { Keys.RightShift, ConsoleAction.UppercaseModifier },
            { Keys.Up, ConsoleAction.PreviousCommandInHistory },
            { Keys.Down, ConsoleAction.NextCommandInHistory },
            { Keys.LeftControl, Keys.X, ConsoleAction.Cut },
            { Keys.RightControl, Keys.X, ConsoleAction.Cut },
            { Keys.LeftControl, Keys.C, ConsoleAction.Copy },
            { Keys.RightControl, Keys.C, ConsoleAction.Copy },
            { Keys.LeftControl, Keys.V, ConsoleAction.Paste },
            { Keys.RightControl, Keys.V, ConsoleAction.Paste },
            { Keys.LeftShift, Keys.Enter, ConsoleAction.NewLine },
            { Keys.RightShift, Keys.Enter, ConsoleAction.NewLine },
            { Keys.LeftControl, Keys.Space, ConsoleAction.AutocompleteForward },
            { Keys.RightControl, Keys.Space, ConsoleAction.AutocompleteForward },
            { Keys.LeftControl, Keys.LeftShift, Keys.Space, ConsoleAction.AutocompleteBackward },
            { Keys.LeftControl, Keys.RightShift, Keys.Space, ConsoleAction.AutocompleteBackward },
            { Keys.RightControl, Keys.LeftShift, Keys.Space, ConsoleAction.AutocompleteBackward },
            { Keys.RightControl, Keys.RightShift, Keys.Space, ConsoleAction.AutocompleteBackward },
            { Keys.Tab, ConsoleAction.Tab },
            { Keys.LeftShift, Keys.Tab, ConsoleAction.RemoveTab },
            { Keys.RightShift, Keys.Tab, ConsoleAction.RemoveTab },
            { Keys.CapsLock, ConsoleAction.CapsLock },
            { Keys.NumLock, ConsoleAction.NumLock },
            { Keys.LeftShift, ConsoleAction.SelectionModifier }, // ?
            { Keys.RightShift, ConsoleAction.SelectionModifier }
        };

        private Dictionary<Keys, Symbol> _symbolDefinitions = new Dictionary<Keys, Symbol>
        {
            // Digits.
            { Keys.D1, new Symbol("1", "!") },
            { Keys.D2, new Symbol("2", "@") },
            { Keys.D3, new Symbol("3", "#") },
            { Keys.D4, new Symbol("4", "$") },
            { Keys.D5, new Symbol("5", "%") },
            { Keys.D6, new Symbol("6", "^") },
            { Keys.D7, new Symbol("7", "&") },
            { Keys.D8, new Symbol("8", "*") },
            { Keys.D9, new Symbol("9", "(") },
            { Keys.D0, new Symbol("0", ")") },
            { Keys.NumPad1, new Symbol("1") },
            { Keys.NumPad2, new Symbol("2") },
            { Keys.NumPad3, new Symbol("3") },
            { Keys.NumPad4, new Symbol("4") },
            { Keys.NumPad5, new Symbol("5") },
            { Keys.NumPad6, new Symbol("6") },
            { Keys.NumPad7, new Symbol("7") },
            { Keys.NumPad8, new Symbol("8") },
            { Keys.NumPad9, new Symbol("9") },
            { Keys.NumPad0, new Symbol("0") },

            // Letters.
            { Keys.Q, new Symbol("q", "Q") },
            { Keys.W, new Symbol("w", "W") },
            { Keys.E, new Symbol("e", "E") },
            { Keys.R, new Symbol("r", "R") },
            { Keys.T, new Symbol("t", "T") },
            { Keys.Y, new Symbol("y", "Y") },
            { Keys.U, new Symbol("u", "U") },
            { Keys.I, new Symbol("i", "I") },
            { Keys.O, new Symbol("o", "O") },
            { Keys.P, new Symbol("p", "P") },
            { Keys.OemOpenBrackets, new Symbol("[", "{") },
            { Keys.OemCloseBrackets, new Symbol("]", "}") },

            { Keys.A, new Symbol("a", "A") },
            { Keys.S, new Symbol("s", "S") },
            { Keys.D, new Symbol("d", "D") },
            { Keys.F, new Symbol("f", "F") },
            { Keys.G, new Symbol("g", "G") },
            { Keys.H, new Symbol("h", "H") },
            { Keys.J, new Symbol("j", "J") },
            { Keys.K, new Symbol("k", "K") },
            { Keys.L, new Symbol("l", "L") },
            { Keys.OemSemicolon, new Symbol(";", ":") },
            { Keys.OemQuotes, new Symbol("'", "\"") },
#if PARADOX
            { Keys.Oem5, new SymbolPair("\\", "|") },            
#else
            { Keys.OemPipe, new Symbol("\\", "|") },
#endif

            { Keys.OemBackslash, new Symbol("\\", "|") },
            { Keys.Z, new Symbol("z", "Z") },
            { Keys.X, new Symbol("x", "X") },
            { Keys.C, new Symbol("c", "C") },
            { Keys.V, new Symbol("v", "V") },
            { Keys.B, new Symbol("b", "B") },
            { Keys.N, new Symbol("n", "N") },
            { Keys.M, new Symbol("m", "M") },
            { Keys.OemComma, new Symbol(",", "<") },
            { Keys.OemPeriod, new Symbol(".", ">") },
#if PARADOX
            { Keys.Oem2, new SymbolPair("/", "?") },
#else
            { Keys.OemQuestion, new Symbol("/", "?") },
#endif

            // Special.
            { Keys.Space, new Symbol(" ", " ") },
            { Keys.OemMinus, new Symbol("-", "_") },
            { Keys.OemPlus, new Symbol("=", "+") },
            { Keys.Decimal, new Symbol(".") },
            { Keys.Add, new Symbol("+") },
            { Keys.Subtract, new Symbol("-") },
            { Keys.Multiply, new Symbol("*") },
            { Keys.Divide, new Symbol("/") },
            //{ Keys.Tab, new SymbolPair("\t", "\t") } // Tab char is not supported in many fonts.
            //{ Keys.Tab, new SymbolPair("    ", "    ") } // Use 4 spaces instead.
        };         
    }

    /// <summary>
    /// Defines which subparts of the <see cref="Console"/> to clear.
    /// </summary>
    [Flags]
    public enum ConsoleClearFlags
    {
        /// <summary>
        /// Does not clear anything.
        /// </summary>
        None = 0,
        /// <summary>
        /// Clears the text in the output part of the console.
        /// </summary>
        OutputBuffer = 1,
        /// <summary>
        /// Clears the text in the input part of the console and resets Caret position.
        /// </summary>
        InputBuffer = 2,
        /// <summary>
        /// Removes any history of user input.
        /// </summary>
        InputHistory = 4,
        /// <summary>
        /// Clears everything.
        /// </summary>
        All = OutputBuffer | InputBuffer | InputHistory
    }

    internal enum ConsoleAction : byte
    {
        None,
        DeletePreviousChar,
        AutocompleteForward,
        AutocompleteBackward,
        ExecuteCommand,
        NewLine,
        UppercaseModifier,
        CopyPasteModifier,
        PreviousEntryModifier,
        AutocompleteModifier,
        MoveByWordModifier,
        CapsLock,
        NumLock,
        Space,
        MoveToEnd,
        MoveToBeginning,
        MoveLeft,
        MoveLeftWord,
        PreviousCommandInHistory,
        MoveRight,
        MoveRightWord,
        NextCommandInHistory,
        //Insert, // input modifier
        DeleteCurrentChar,
        //NumLock,      
        Clear,
        Cut,
        Copy,
        Paste,
        Tab,
        RemoveTab,
        SelectionModifier
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
        public Color BottomBorderColor { get; set; } = Color.Red;
        public float BottomBorderThickness { get; set; } = 0.0f;
        public Texture2D BackgroundTexture { get; set; }
        public Matrix BackgroundTextureTransform { get; set; } = Matrix.Identity;
        public Color SelectionColor { get; set; } = new Color(1.0f, 1.0f, 0.0f, 0.5f);
        public string TabSymbol { get; set; } = "    ";
        public bool TextSelectionEnabled { get; set; } = true;
    }
}