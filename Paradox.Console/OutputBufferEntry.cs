using System;
using System.Collections.Generic;
using Varus.Paradox.Console.Utilities;

namespace Varus.Paradox.Console
{
    internal class OutputBufferEntry
    {
        private readonly OutputBuffer _viewBuffer;        

        public OutputBufferEntry(OutputBuffer viewBuffer)
        {
            _viewBuffer = viewBuffer;
            Lines = new List<string>();
            Value = "";
        }

        public string Value { get; private set; }
        public List<string> Lines { get; private set; }

        public int SetValueAndCalculateLines(string value, float screenWidth, bool countPrefix)
        {            
            Value = value.Replace("\t", _viewBuffer.ConsolePanel.Tab);
            return CalculateLines(screenWidth, countPrefix);
        }

        public int CalculateLines(float bufferAreaWidth, bool countPrefix)
        {
            Lines.Clear();
            string[] values = Value.Split(_viewBuffer.ConsolePanel.NewLine.AsArray(), StringSplitOptions.None);                        
            for (int i = 0; i < values.Length; i++)
            {
                CalculateLinesPart(values[i], bufferAreaWidth, i == 0 && countPrefix);
            }

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
                if (!_viewBuffer.ConsolePanel.CharWidthMap.TryGetValue(c, out charWidth))
                {
                    charWidth += _viewBuffer.ConsolePanel.Font.MeasureString(c.ToString()).X;
                    _viewBuffer.ConsolePanel.CharWidthMap.Add(c, charWidth);
                }

                if (countPrefix)
                    charWidth += _viewBuffer.ConsolePanel.InputBuffer.InputPrefixSize.X;

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
