using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Input;
using Varus.Paradox.Console.Utilities;

namespace Varus.Paradox.Console
{
    public partial class Console
    {
        private readonly BiDirectionalDictionary<Keys, ConsoleAction> _actionDefinitions = new BiDirectionalDictionary<Keys, ConsoleAction>
        {
            { Keys.Enter, ConsoleAction.ExecuteCommand },
            { Keys.NumPadEnter, ConsoleAction.ExecuteCommand },
            { Keys.Left, ConsoleAction.MoveLeft },
            { Keys.Right, ConsoleAction.MoveRight },
            { Keys.Home, ConsoleAction.MoveToBeginning },
            { Keys.End, ConsoleAction.MoveToEnd },
            { Keys.Back, ConsoleAction.DeletePreviousChar },
            { Keys.Delete, ConsoleAction.DeleteCurrentChar },
            { Keys.LeftShift, ConsoleAction.UppercaseModifier },
            { Keys.Up, ConsoleAction.PreviousCommandInHistory },
            { Keys.Down, ConsoleAction.NextCommandInHistory },            
            { Keys.LeftCtrl, ConsoleAction.CopyPasteModifier },
            { Keys.LeftShift, ConsoleAction.PreviousEntryModifier },
            { Keys.V, ConsoleAction.Paste },
            { Keys.LeftShift, ConsoleAction.NextLineModifier },
            //{ Keys.Tab, ConsoleAction.Autocomplete },            
            { Keys.Space, ConsoleAction.Autocomplete },
            { Keys.LeftCtrl, ConsoleAction.AutocompleteModifier },
            { Keys.Tab, ConsoleAction.Tab },
            { Keys.LeftShift, ConsoleAction.TabModifier }
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
            { Keys.Oem5, new SymbolPair("\\", "|") },            

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
            { Keys.Oem2, new SymbolPair("/", "?") },

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

        private readonly ConsoleSettings _defaultSettings = new ConsoleSettings
        {
            BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f),
            FontColor = new Color(1.0f, 1.0f, 0.0f, 1.0f),
            OpenCloseTransitionSeconds = 0.25f,
            TimeUntilRepeatingInput = 0.4f,
            RepeatingInputCooldown = 0.04f,
            HeightRatio = 0.4f,
            Enabled = true,
            Visible = true,
            InputPrefix = "]",
            NumPositionsToMoveWhenOutOfScreen = 4,
            InputPrefixColor = Color.Yellow,
            Padding = 2,
            CaretSymbol = "_",
            CaretBlinkingIntervalSeconds = 0.4f
        };
    }
}
