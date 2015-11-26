using System;
using System.Collections.Generic;
using QuakeConsole.Utilities;

namespace QuakeConsole.Output
{
    internal class OutputEntry
    {
        private readonly ConsoleOutput _view;
        private string _value = "";

        public OutputEntry(ConsoleOutput view)
        {
            _view = view;
        }        

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value.Replace("\t", _view.Console.TabSymbol);
                Lines.Clear();
                Lines.Add(_value);
            }
        }

        public List<string> Lines { get; } = new List<string>();

        public int CalculateLines(float bufferAreaWidth, bool countPrefix)
        {
            Lines.Clear();
            string[] values = Value.Split(_view.Console.NewlineSymbol, StringSplitOptions.None);                        
            for (int i = 0; i < values.Length; i++)
                CalculateLinesPart(values[i], bufferAreaWidth, i == 0 && countPrefix);

            return Lines.Count;
        }

        public override string ToString() => Value;        

        private void CalculateLinesPart(string value, float bufferAreaWidth, bool countPrefix)
        {
            float lineWidthProgress = 0;
            int startIndex = 0;
            int length = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                float charWidth;
                if (!_view.Console.CharWidthMap.TryGetValue(c, out charWidth))
                {
                    charWidth += _view.Console.Font.MeasureString(c.ToString()).X;
                    _view.Console.CharWidthMap.Add(c, charWidth);
                }

                if (countPrefix)
                    charWidth += _view.Console.ConsoleInput.InputPrefixSize.X;

                if (lineWidthProgress + charWidth > bufferAreaWidth)
                {
                    Lines.Add(value.Substring(startIndex, length));
                    length = 0;
                    lineWidthProgress = 0;
                    startIndex = i;
                }

                lineWidthProgress += charWidth;
                length++;
            }

            // Add last row.
            Lines.Add(value.Substring(startIndex, length));
        }
    }
}
