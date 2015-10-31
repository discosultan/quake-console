using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using QuakeConsole.Utilities;

namespace QuakeConsole.Input.Features
{
    internal class MultiLineInput
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        private Pool<InputEntry> _inputEntryPool;
        private ConsoleInput _input;

        public bool Enabled { get; set; } = true;

        public int ActiveLineIndex { get; private set; }
        public List<InputEntry> InputLines { get; } = new List<InputEntry>();
        public InputEntry ActiveLine => InputLines[ActiveLineIndex];                

        public void LoadContent(ConsoleInput input)
        {
            _input = input;
            _inputEntryPool = new Pool<InputEntry>(() => new InputEntry(_input));

            Clear(); // Also sets up first line.
        }

        public void Clear()
        {
            foreach (InputEntry entry in InputLines)
                _inputEntryPool.Release(entry);
            InputLines.Clear();
            InputLines.Add(_inputEntryPool.Fetch());
            ActiveLineIndex = 0;
        }

        public string GetInput()
        {
            _stringBuilder.Clear();
            for (int i = 0; i < InputLines.Count; i++)
            {
                _stringBuilder.Append(InputLines[i].Value);
                if (i != InputLines.Count - 1)
                    _stringBuilder.Append(_input.Console.NewlineSymbol);
            }
            return _stringBuilder.ToString();
        }

        public void RemoveActiveLine()
        {
            if (ActiveLineIndex == 0) return;

            InputEntry lineToRemove = ActiveLine;
            InputLines.RemoveAt(ActiveLineIndex);
            ActiveLineIndex--;
            _input.Caret.Index = ActiveLine.Buffer.Length;
            ActiveLine.Buffer.Append(lineToRemove.Buffer);            
            _inputEntryPool.Release(lineToRemove);
        }

        public void RemoveNextLine()
        {
            int nextIndex = ActiveLineIndex + 1;

            if (InputLines.Count <= nextIndex)
                return;            

            InputEntry entryToRemove = InputLines[nextIndex];
            // Move any text from next line to current line.
            ActiveLine.Buffer.Append(entryToRemove.Value);
            InputLines.RemoveAt(nextIndex);
            _inputEntryPool.Release(entryToRemove);
        }

        public void AddNewLine(string value)
        {
            if (value == null) return;
            
            string stringToMoveToNextLine = ActiveLine.Buffer.Substring(_input.Caret.Index);

            InputEntry entry = _inputEntryPool.Fetch();
            entry.Value = value;
            InputLines.Add(entry);
            ActiveLineIndex++;
            _input.Caret.MoveBy(int.MaxValue);
            entry.Buffer.Append(stringToMoveToNextLine);            
        }

        public void OnAction(ConsoleAction action)
        {
            if (!Enabled) return;

            switch (action)
            {
                case ConsoleAction.NewLine:                    
                    InputEntry previousActiveLine = ActiveLine;
                    int previousCaretIndex = _input.CaretIndex;
                    AddNewLine("");
                    // Push everything after caret to newline.
                    //AddNewLine(previousActiveLine.Buffer.Substring(_input.CaretIndex));
                    // Leave everything before caret to existing line.                    
                    previousActiveLine.Buffer.Remove(previousCaretIndex);                                                            
                    break;
                case ConsoleAction.MovePreviousLine:
                    ActiveLineIndex = Math.Max(ActiveLineIndex - 1, 0);
                    _input.Caret.Index = Math.Min(_input.Caret.Index, ActiveLine.Buffer.Length);
                    break;
                case ConsoleAction.MoveNextLine:
                    ActiveLineIndex = Math.Min(ActiveLineIndex + 1, InputLines.Count - 1);
                    _input.Caret.Index = Math.Min(_input.Caret.Index, ActiveLine.Buffer.Length);
                    break;
            }
        }

        public void Draw()
        {
            Console console = _input.Console;

            // Draw from bottom to top.
            var viewPosition = new Vector2(
                console.Padding,
                console.WindowArea.Y + console.WindowArea.Height - console.Padding - console.FontSize.Y);

            for (int i = 0; i < InputLines.Count; i++)
            {
                if (i + 1 > _input.Console.TotalNumberOfVisibleLines)
                    break;
                DrawRow(InputLines[i], ref viewPosition, true);
            }
        }

        private void DrawRow(InputEntry entry, ref Vector2 viewPosition, bool drawPrefix)
        {
            Console console = _input.Console;
            Vector2 tempViewPos = viewPosition;
            if (drawPrefix)
            {
                console.SpriteBatch.DrawString(
                    console.Font,
                    _input.InputPrefix,
                    tempViewPos,
                    _input.InputPrefixColor);
                tempViewPos.X += _input.InputPrefixSize.X;
            }
            console.SpriteBatch.DrawString(
                console.Font,
                entry.Value,
                tempViewPos,
                console.FontColor);
            viewPosition.Y -= console.FontSize.Y;
        }        
    }
}
