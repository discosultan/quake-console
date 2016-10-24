using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace QuakeConsole
{
    /// <summary>
    /// An action or modifier (other than symbol input) available in the console.
    /// </summary>
    public enum ConsoleAction : byte
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        None,
        DeletePreviousChar,
        DeleteCurrentChar,
        AutocompleteForward,
        AutocompleteBackward,
        ExecuteCommand,
        //MovePreviousLine,
        //MoveNextLine,
        NewLine,
        CapsLock,
        //NumLock,
        Space,
        MoveLeft,
        MoveLeftWord,
        MoveToBeginning,
        MoveRight,
        MoveRightWord,
        MoveToEnd,
        PreviousCommandInHistory,
        NextCommandInHistory,
        Cut,
        Copy,
        Paste,
        Tab,
        RemoveTab,
        UppercaseModifier,
        SelectionModifier
#pragma warning restore CS1591
    }

    /// <summary>
    /// A map specifying which input keys are translated to which <see cref="ConsoleAction"/>s.
    /// </summary>
    /// <remarks>
    /// <see cref="IEnumerable" /> is implemented only to allow collection initializer syntax. 
    /// Iteration will fail!
    /// </remarks>
    public class ConsoleActionMap : IEnumerable
    {
        private readonly BiDirectionalMultiValueDictionary<Int3, ConsoleAction> _map = new BiDirectionalMultiValueDictionary<Int3, ConsoleAction>();

        /// <summary>
        /// Adds a mapping from keyboard key to <see cref="ConsoleAction"/>
        /// </summary>
        public void Add(Keys key, ConsoleAction action) =>
            _map.Add(new Int3((int)Keys.None, (int)Keys.None, (int)key), action);

        /// <summary>
        /// Adds a mapping from keyboard keys to <see cref="ConsoleAction"/>
        /// </summary>
        public void Add(Keys modifier, Keys key, ConsoleAction action) =>
            _map.Add(new Int3((int)Keys.None, (int)modifier, (int)key), action);

        /// <summary>
        /// Adds a mapping from keyboard keys to <see cref="ConsoleAction"/>
        /// </summary>
        public void Add(Keys modifier1, Keys modifier2, Keys key, ConsoleAction action) =>
            _map.Add(new Int3((int)modifier1, (int)modifier2, (int)key), action);        

        /// <summary>
        /// Removes a mapping from keyboard key to <see cref="ConsoleAction"/>
        /// </summary>
        public void Remove(Keys key) =>
            _map.Remove(new Int3((int)Keys.None, (int)Keys.None, (int)key));

        /// <summary>
        /// Removes a mapping from keyboard keys to <see cref="ConsoleAction"/>
        /// </summary>
        public void Remove(Keys modifier, Keys key) =>
            _map.Remove(new Int3((int)Keys.None, (int)modifier, (int)key));

        /// <summary>
        /// Removes a mapping from keyboard keys to <see cref="ConsoleAction"/>
        /// </summary>
        public void Remove(Keys modifier1, Keys modifier2, Keys key) =>
            _map.Remove(new Int3((int)modifier1, (int)modifier2, (int)key));

        /// <summary>
        /// Removes all mappings to <see cref="ConsoleAction"/>
        /// </summary>
        public void Remove(ConsoleAction action) =>
            _map.Remove(action);

        internal bool AreModifiersAppliedForAction(ConsoleAction action, InputState input)
        {
            bool modifiersAccepted = false;
            List<Int3> requiredModifiers;
            if (_map.BackwardTryGetValues(action, out requiredModifiers))
            {                
                foreach (Int3 modifiers in requiredModifiers)
                {
                    var modifier1 = (Keys)modifiers.X;
                    var modifier2 = (Keys)modifiers.Y;

                    modifiersAccepted = modifiersAccepted ||
                        (modifier1 == Keys.None || input.IsKeyDown(modifier1)) &&
                        (modifier2 == Keys.None || input.IsKeyDown(modifier2));
                }
            }
            return modifiersAccepted;
        }

        internal bool TryGetAction(InputState input, out ConsoleAction action)
        {                        
            foreach (Keys key in input.PressedKeys)
            {
                // First look for actions with two modifiers.
                foreach (Keys modifier1 in input.DownKeys)
                    foreach (Keys modifier2 in input.DownKeys)
                    if (_map.ForwardTryGetValue(new Int3((int)modifier1, (int)modifier2, (int)key), out action))
                        return true;

                // Then look for actions with one modifier.
                foreach (Keys modifier in input.DownKeys)
                    if (_map.ForwardTryGetValue(new Int3((int)Keys.None, (int)modifier, (int)key), out action))
                        return true;
            }
                
            // If not found; look for actions without modifiers.
            foreach (Keys key in input.PressedKeys)
                if (_map.ForwardTryGetValue(new Int3((int)Keys.None, (int)Keys.None, (int)key), out action))
                    return true;

            action = default(ConsoleAction);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
