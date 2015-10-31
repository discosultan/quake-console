using System.Linq;
using Microsoft.Xna.Framework;
using QuakeConsole.Input;
using QuakeConsole.Utilities;

namespace QuakeConsole.Output
{
    /// <summary>
    /// Output part of the <see cref="Console"/>. Command execution info will be appended here.
    /// </summary>
    internal class ConsoleOutput : IConsoleOutput
    {                
        private readonly CircularArray<OutputEntry> _entries = new CircularArray<OutputEntry>();

        private Pool<OutputEntry> _entryPool;
        
        private int _numRows;
        private bool _removeOverflownEntries;

        public void LoadContent(Console console)
        {
            Console = console;
            _entryPool = new Pool<OutputEntry>(() => new OutputEntry(this));
            
            Console.PaddingChanged += (s, e) =>
            {
                CalculateNumberOfLines();
                RemoveOverflownBufferEntriesIfAllowed();
            };
            Console.FontChanged += (s, e) =>
            {
                CalculateNumberOfLines();
                RemoveOverflownBufferEntriesIfAllowed();
            };
            Console.WindowAreaChanged += (s, e) =>
            {
                CalculateNumberOfLines();
                RemoveOverflownBufferEntriesIfAllowed();
            };
            CalculateNumberOfLines();
        }
        
        public bool RemoveOverflownEntries 
        {
            get { return _removeOverflownEntries; }
            set 
            { 
                _removeOverflownEntries = value;
                RemoveOverflownBufferEntriesIfAllowed();
            }
        }

        public Console Console { get; private set; }

        /// <summary>
        /// Appends a message to the buffer.
        /// </summary>
        /// <param name="message">Message to append.</param>
        public void Append(string message)
        {
            if (message == null) return;            

            var viewBufferEntry = _entryPool.Fetch();
            viewBufferEntry.Value = message;            
            _numRows += viewBufferEntry.CalculateLines(Console.WindowArea.Width - Console.Padding * 2, false);
            _entries.Enqueue(viewBufferEntry);
            RemoveOverflownBufferEntriesIfAllowed();
        }

        /// <summary>
        /// Clears all the information in the buffer.
        /// </summary>
        public void Clear()
        {
            foreach (OutputEntry entry in _entries)
                _entryPool.Release(entry);
            _entries.Clear();
        }

        public void Draw()
        {
            int indexOffset = Console.LineIndexAfterInput + 1; 

            // Draw from bottom to top.
            var viewPosition = new Vector2(
                Console.Padding, 
                Console.WindowArea.Y + Console.WindowArea.Height - Console.Padding - Console.FontSize.Y * indexOffset);

            int rowCounter = 0;
            for (int i = _entries.Length - 1; i >= 0; i--)
            {
                if (rowCounter >= Console.NumberOfAvailableLinesAfterInput) break;
                DrawRow(_entries[i], ref viewPosition, ref rowCounter, false);
            }
        }

        public void SetDefaults(ConsoleSettings settings)
        {
        }

        private void DrawRow(OutputEntry entry, ref Vector2 viewPosition, ref int rowCounter, bool drawPrefix)
        {            
            for (int j = entry.Lines.Count - 1; j >= 0; j--)
            {
                Vector2 tempViewPos = viewPosition;
                if (drawPrefix)
                {
                    Console.SpriteBatch.DrawString(
                        Console.Font,
                        Console.ConsoleInput.InputPrefix,
                        tempViewPos,
                        Console.ConsoleInput.InputPrefixColor);
                    tempViewPos.X += Console.ConsoleInput.InputPrefixSize.X;
                }
                Console.SpriteBatch.DrawString(
                    Console.Font,
                    entry.Lines[j],
                    tempViewPos, 
                    Console.FontColor);
                viewPosition.Y -= Console.FontSize.Y;
                rowCounter++;
            }
        }

        private void RemoveOverflownBufferEntriesIfAllowed()
        {
            if (!RemoveOverflownEntries) return;

            int maxNumRows = Console.NumberOfAvailableLinesAfterInput;
            while (_numRows > maxNumRows)
            {
                OutputEntry entry = _entries.Peek();

                // Remove entry only if it is completely hidden from view.
                if (_numRows - entry.Lines.Count >= maxNumRows)
                {
                    _numRows -= entry.Lines.Count;
                    _entries.Dequeue();
                    _entryPool.Release(entry);
                }
                else
                {
                    break;
                }
            }
        }

        private void CalculateNumberOfLines()
        {
            _numRows = _entries.Sum(entry => entry.CalculateLines(Console.WindowArea.Width - Console.Padding * 2, false));
        }
    }
}
