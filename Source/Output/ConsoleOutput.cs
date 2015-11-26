using System.Linq;
using QuakeConsole.Utilities;
using System.Collections.Generic;
using System.Text;
using System;
#if MONOGAME
using Microsoft.Xna.Framework;
#endif

namespace QuakeConsole.Output
{
    /// <summary>
    /// Output part of the <see cref="Console"/>. Command execution info and results will be appended here.
    /// </summary>
    internal class ConsoleOutput : IConsoleOutput
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly CircularArray<OutputEntry> _entries = new CircularArray<OutputEntry>();
        private readonly List<OutputEntry> _commandEntries = new List<OutputEntry>();

        private Pool<OutputEntry> _entryPool;

        private int _maxNumRows;
        private int _numRows;
        private bool _removeOverflownEntries;

        public void LoadContent(Console console)
        {
            Console = console;
            _entryPool = new Pool<OutputEntry>(() => new OutputEntry(this));
            
            Console.PaddingChanged += (s, e) =>
            {
                CalculateRows();
                RemoveOverflownBufferEntriesIfAllowed();
            };
            Console.FontChanged += (s, e) =>
            {
                CalculateRows();
                RemoveOverflownBufferEntriesIfAllowed();
            };
            Console.WindowAreaChanged += (s, e) =>
            {
                CalculateRows();
                RemoveOverflownBufferEntriesIfAllowed();
            };
            CalculateRows();
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

        internal bool HasCommandEntry => _commandEntries.Count > 0;

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
            foreach (OutputEntry entry in _commandEntries)
                _entryPool.Release(entry);
            _commandEntries.Clear();
        }

        public void AddCommandEntry(string value)
        {
            if (value == null) return;

            var entry = _entryPool.Fetch();
            entry.Value = value;
            _numRows++;
            //entry.CalculateLines(_console.WindowArea.Width - _console.Padding * 2, true);
            _commandEntries.Add(entry);
        }

        public string DequeueCommandEntry()
        {
            _stringBuilder.Clear();
            for (int i = 0; i < _commandEntries.Count; i++)
            {
                OutputEntry entry = _commandEntries[i];
                _stringBuilder.Append(entry.Value);
                //if (i != _commandEntries.Count - 1)
                _stringBuilder.Append(Console.NewlineSymbol);
                _entryPool.Release(entry);
            }
            _commandEntries.Clear();
            return _stringBuilder.ToString();
        }

        public void SetDefaults(ConsoleSettings settings)
        {
        }

        public void Draw()
        {
            // Draw from bottom to top.
            var viewPosition = new Vector2(
                Console.Padding,
                Console.WindowArea.Y + Console.WindowArea.Height - Console.Padding - Console.ConsoleInput.InputPrefixSize.Y - Console.FontSize.Y);

            int rowCounter = 0;

            for (int i = _commandEntries.Count - 1; i >= 0; i--)
            {
                if (rowCounter >= _maxNumRows) return;
                DrawRow(_commandEntries[i], ref viewPosition, ref rowCounter, true);
            }

            for (int i = _entries.Length - 1; i >= 0; i--)
            {
                if (rowCounter >= _maxNumRows) return;
                DrawRow(_entries[i], ref viewPosition, ref rowCounter, false);
            }
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

            while (_numRows > _maxNumRows)
            {
                OutputEntry entry = _entries.Peek();

                // Remove entry only if it is completely hidden from view.
                if (_numRows - entry.Lines.Count >= _maxNumRows)
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

        private void CalculateRows()
        {
            // Take top padding into account and hide any row which is only partly visible.
            //_maxNumRows = Math.Max((int)((_console.WindowArea.Height - _console.Padding * 2) / _fontSize.Y) - 1, 0);            

            // Disregard top padding and allow any row which is only partly visible.
            _maxNumRows = Math.Max((int)Math.Ceiling(((Console.WindowArea.Height - Console.Padding) / Console.FontSize.Y)) - 1, 0);

            _numRows = _commandEntries.Count + /*GetNumRows(_commandEntries) +*/ GetNumRows(_entries);
        }

        private int GetNumRows(IEnumerable<OutputEntry> collection)
        {
            return collection.Sum(entry => entry.CalculateLines(Console.WindowArea.Width - Console.Padding * 2, false));
        }
    }
}
