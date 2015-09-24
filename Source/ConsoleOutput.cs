using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuakeConsole.Utilities;
#if MONOGAME
using Microsoft.Xna.Framework;
#endif

namespace QuakeConsole
{
    /// <summary>
    /// Output part of the <see cref="Console"/>. Command execution info will be appended here.
    /// </summary>
    public class ConsoleOutput : IConsoleOutput
    {
        private const string MeasureFontSizeSymbol = "x";
        
        private readonly CircularArray<OutputEntry> _entries = new CircularArray<OutputEntry>();
        private readonly List<OutputEntry> _commandEntries = new List<OutputEntry>();        
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        private Console _console;
        private Pool<OutputEntry> _entryPool;

        private Vector2 _fontSize;
        private int _maxNumRows;
        private int _numRows;           
        private bool _removeOverflownEntries;        

        internal void LoadContent(Console console)
        {
            _console = console;
            _entryPool = new Pool<OutputEntry>(() => new OutputEntry(this));

            // TODO: Set flags only and do any calculation in Update. While this would win in performance
            // TODO: in some cases, I'm not convinced it's worth the hit against readability.
            _console.PaddingChanged += (s, e) =>
            {
                CalculateRows();
                RemoveOverflownBufferEntriesIfAllowed();
            };
            _console.FontChanged += (s, e) =>
            {
                CalculateFontSize();
                CalculateRows();
                RemoveOverflownBufferEntriesIfAllowed();
            };
            _console.WindowAreaChanged += (s, e) =>
            {
                CalculateRows();
                RemoveOverflownBufferEntriesIfAllowed();
            };

            CalculateFontSize();
            CalculateRows();
        }

        /// <summary>
        /// Gets or sets if rows which run out of the visible area of the console should be removed.
        /// </summary>
        public bool RemoveOverflownEntries 
        {
            get { return _removeOverflownEntries; }
            set 
            { 
                _removeOverflownEntries = value;
                RemoveOverflownBufferEntriesIfAllowed();
            }
        }

        internal Console Console => _console;

        internal bool HasCommandEntry => _commandEntries.Count > 0;

        /// <summary>
        /// Appends a message to the buffer.
        /// </summary>
        /// <param name="message">Message to append.</param>
        public void Append(string message)
        {
            if (message == null) return;            

            var viewBufferEntry = _entryPool.Fetch();
            _numRows += viewBufferEntry.SetValueAndCalculateLines(message, _console.WindowArea.Width - _console.Padding * 2, false);
            _entries.Enqueue(viewBufferEntry);
            RemoveOverflownBufferEntriesIfAllowed();
        }

        /// <summary>
        /// Clears all the information in the buffer.
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
            _commandEntries.Clear();
        }

        internal void AddCommandEntry(string value)
        {
            if (value == null) return;

            var entry = _entryPool.Fetch();
            entry.SetValueAndCalculateLines(value, _console.WindowArea.Width - _console.Padding * 2, true);
            _commandEntries.Add(entry);
        }

        internal string DequeueCommandEntry()
        {
            _stringBuilder.Clear();
            for (int i = 0; i < _commandEntries.Count; i++)
            {
                _stringBuilder.Append(_commandEntries[i].Value);
                //if (i != _commandEntries.Count - 1)
                _stringBuilder.Append("\n");
            }
            _commandEntries.Clear();
            return _stringBuilder.ToString();
        }        

        internal void Draw()
        {
            // Draw from bottom to top.
            var viewPosition = new Vector2(
                _console.Padding, 
                _console.WindowArea.Y + _console.WindowArea.Height - _console.Padding - _console.ConsoleInput.InputPrefixSize.Y - _fontSize.Y);

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

        internal void SetDefaults(ConsoleSettings settings)
        {
        }

        private void DrawRow(OutputEntry entry, ref Vector2 viewPosition, ref int rowCounter, bool drawPrefix)
        {            
            for (int j = entry.Lines.Count - 1; j >= 0; j--)
            {
                Vector2 tempViewPos = viewPosition;
                if (drawPrefix)
                {
                    _console.SpriteBatch.DrawString(
                        _console.Font,
                        _console.ConsoleInput.InputPrefix,
                        tempViewPos,
                        _console.ConsoleInput.InputPrefixColor);
                    tempViewPos.X += _console.ConsoleInput.InputPrefixSize.X;
                }
                _console.SpriteBatch.DrawString(
                    _console.Font,
                    entry.Lines[j],
                    tempViewPos, 
                    _console.FontColor);
                viewPosition.Y -= _fontSize.Y;
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
            _maxNumRows = Math.Max((int)Math.Ceiling(((_console.WindowArea.Height - _console.Padding) / _fontSize.Y)) - 1, 0);
            
            _numRows = GetNumRows(_commandEntries) + GetNumRows(_entries);
        }

        private int GetNumRows(IEnumerable<OutputEntry> collection)
        {
            return collection.Sum(entry => entry.CalculateLines(_console.WindowArea.Width - _console.Padding * 2, false));
        }

        private void CalculateFontSize()
        {
            _fontSize = _console.Font.MeasureString(MeasureFontSizeSymbol);
        }        
    }
}
