using System;
#if MONOGAME
using MathUtil = Microsoft.Xna.Framework.MathHelper;
#endif

namespace QuakeConsole.Input
{
    internal class InputEntry
    {
        private readonly ConsoleInput _input;

        public InputEntry(ConsoleInput input)
        {
            _input = input;
        }

        public string Value { get; set; } = "";

        public int VisibleStartIndex { get; private set; }
        public int VisibleLength { get; private set; }

        private void CalculateStartAndEndIndices()
        {
            Console console = _input.Console;

            float windowWidth = console.WindowArea.Width - console.Padding * 2 - _input.InputPrefixSize.X;

            if (_input.Caret.Index > _input.Length - 1)
                windowWidth -= _input.Caret.Width;

            while (_input.Caret.Index <= VisibleStartIndex && VisibleStartIndex > 0)
                VisibleStartIndex = Math.Max(VisibleStartIndex - _input.NumPositionsToMoveWhenOutOfScreen, 0);

            VisibleLength = MathUtil.Min(VisibleLength, _input.Length - VisibleStartIndex - 1);

            float widthProgress = 0f;
            int indexer = VisibleStartIndex;
            int targetIndex = _input.Caret.Index;
            while (indexer < _input.Length)
            {
                char c = _input[indexer++];

                float charWidth;
                if (!console.CharWidthMap.TryGetValue(c, out charWidth))
                {
                    charWidth = console.Font.MeasureString(c.ToString()).X;
                    console.CharWidthMap.Add(c, charWidth);
                }

                widthProgress += charWidth;

                if (widthProgress > windowWidth)
                {
                    if (targetIndex >= VisibleStartIndex && targetIndex - VisibleStartIndex < VisibleLength || indexer - 1 == VisibleStartIndex) break;

                    if (targetIndex >= VisibleStartIndex)
                    {
                        VisibleStartIndex += _input.NumPositionsToMoveWhenOutOfScreen;
                        VisibleStartIndex = Math.Min(VisibleStartIndex, _input.Length - 1);
                    }
                    CalculateStartAndEndIndices();
                    break;
                }

                VisibleLength = indexer - VisibleStartIndex;
            }
        }
    }
}
