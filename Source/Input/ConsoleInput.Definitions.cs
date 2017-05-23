using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace QuakeConsole
{
    internal partial class ConsoleInput
    {
        internal ConsoleActionMap ActionDefinitions { get; } = new ConsoleActionMap
        {
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
            { Keys.LeftShift, Keys.None, ConsoleAction.UppercaseModifier }, // ?
            { Keys.RightShift, Keys.None, ConsoleAction.UppercaseModifier },
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
            //{ Keys.LeftShift, Keys.Up, ConsoleAction.MovePreviousLine },
            //{ Keys.RightShift, Keys.Up, ConsoleAction.MovePreviousLine },
            //{ Keys.LeftShift, Keys.Down, ConsoleAction.MoveNextLine },
            //{ Keys.RightShift, Keys.Down, ConsoleAction.MoveNextLine },
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
            //{ Keys.NumLock, ConsoleAction.NumLock },
            { Keys.LeftShift, Keys.None, ConsoleAction.SelectionModifier }, // ?
            { Keys.RightShift, Keys.None, ConsoleAction.SelectionModifier }
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
            { Keys.OemPipe, new Symbol("\\", "|") },

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
            { Keys.OemQuestion, new Symbol("/", "?") },

            // Special.
            { Keys.Space, new Symbol(" ", " ") },
            { Keys.OemMinus, new Symbol("-", "_") },
            { Keys.OemPlus, new Symbol("=", "+") },
            { Keys.Decimal, new Symbol(".") },
            { Keys.Add, new Symbol("+") },
            { Keys.Subtract, new Symbol("-") },
            { Keys.Multiply, new Symbol("*") },
            { Keys.Divide, new Symbol("/") }
            //{ Keys.Tab, new SymbolPair("\t", "\t") } // Tab char is not supported in many fonts.
            //{ Keys.Tab, new SymbolPair("    ", "    ") } // Use 4 spaces instead.
        };
    }
}