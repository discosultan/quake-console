using System;
using System.Collections.Generic;
using QuakeConsole.Utilities;

namespace QuakeConsole
{
    internal class OutputEntry
    {
        private readonly ConsoleOutput _view;        

        public OutputEntry(ConsoleOutput view)
        {
            _view = view;                        
        }

        public string Value { get; private set; } = "";
        public List<string> Lines { get; } = new List<string>();

        public int SetValueAndCalculateLines(string value, float screenWidth, bool countPrefix)
        {            
            Value = value.Replace("\t", _view.Console.Tab);
            return CalculateLines(screenWidth, countPrefix);
        }

        public int CalculateLines(float bufferAreaWidth, bool countPrefix)
        {
            Lines.Clear();
            string[] values = Value.Split(_view.Console.NewLine, StringSplitOptions.None);                        
            for (int i = 0; i < values.Length; i++)
                CalculateLinesPart(values[i], bufferAreaWidth, i == 0 && countPrefix);

            //// Get the total width required to render the value.
            //float width = font.MeasureString(_value).X;
            //// Get the decimal value of number of lines required to render the value with our screen width.
            //float parts = width / bufferAreaWidth;
            //// Get the integral number of lines required to render the value with our screen width.
            //var numChunks = (int) Math.Ceiling(parts);
            //// Get the theoretical number of chars a line can fit.
            //var theoreticalNumCharsOnLine = (int)(_value.Length / parts);

            //int valueIndexer = 0;
            //for (int i = 0; i < numChunks - 1; i++)
            //{
            //    int numCharsOnCurrentLine = theoreticalNumCharsOnLine;
            //    string subStr;
            //    do
            //    {
            //        subStr = _value.Substring(valueIndexer, numCharsOnCurrentLine);
            //        float subStrWidth = font.MeasureString(subStr).X;
            //        if (subStrWidth > bufferAreaWidth)
            //        {
            //            numCharsOnCurrentLine--;
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    } while (numCharsOnCurrentLine > 0);

            //    valueIndexer += numCharsOnCurrentLine;
            //    Lines.Add(subStr ?? "");                
            //}

            //// Add last line.
            //Lines.Add(_value.Substring(valueIndexer));

            return Lines.Count;
        }

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
