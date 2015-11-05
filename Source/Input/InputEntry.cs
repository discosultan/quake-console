using System;
using QuakeConsole.Utilities;
#if MONOGAME
using MathUtil = Microsoft.Xna.Framework.MathHelper;
#endif

namespace QuakeConsole.Input
{
    internal class InputEntry
    {
        private readonly ConsoleInput _input;

        private bool _dirty = true;

        public InputEntry(ConsoleInput input)
        {
            _input = input;
            input.PrefixChanged += (s, e) => SetDirty();
            input.Console.FontChanged += (s, e) =>
            {
                SetDirty();
                Buffer.Font = _input.Console.Font;
            };            
            input.Console.WindowAreaChanged += (s, e) => SetDirty();
            input.Caret.Moved += (s, e) => SetDirty(); // TODO: refactor out.

            Buffer.Font = _input.Console.Font;
        }

        public SpriteFontStringBuilder Buffer { get; } = new SpriteFontStringBuilder();

        public string Value
        {
            get { return Buffer.ToString(); } // Does not allocate if value is cached.
            set
            {
                Clear();
                if (value != null)
                    Buffer.Append(value);
            }
        }

        public void Clear()
        {
            Buffer.Clear();
            SetDirty();
        }

        private int _visibleStartIndex;
        private int _visibleLength;

        public int VisibleStartIndex
        {
            get
            {
                if (_dirty)
                {
                    CalculateStartAndEndIndices();
                    _dirty = false;
                }
                return _visibleStartIndex;
            }
        }

        public int VisibleLength
        {
            get
            {
                if (_dirty)
                {
                    CalculateStartAndEndIndices();
                    _dirty = false;
                }
                return _visibleLength;
            }
        }

        private void SetDirty() => _dirty = true;

        private void CalculateStartAndEndIndices()
        {
            Console console = _input.Console;

            float windowWidth = console.WindowArea.Width - console.Padding * 2 - _input.InputPrefixSize.X;

            if (_input.CaretIndex > _input.Length - 1)
                windowWidth -= _input.Caret.Width;

            while (_input.CaretIndex <= _visibleStartIndex && _visibleStartIndex > 0)
                _visibleStartIndex = Math.Max(_visibleStartIndex - _input.NumPositionsToMoveWhenOutOfScreen, 0);

            // TODO: ensure _visibleLength not less than 0.
            _visibleLength = MathUtil.Min(_visibleLength, _input.Length - _visibleStartIndex - 1);

            float widthProgress = 0f;
            int indexer = _visibleStartIndex;
            int targetIndex = _input.CaretIndex;
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
                    if (targetIndex >= _visibleStartIndex && targetIndex - _visibleStartIndex < _visibleLength || indexer - 1 == _visibleStartIndex) break;

                    if (targetIndex >= _visibleStartIndex)
                    {
                        _visibleStartIndex += _input.NumPositionsToMoveWhenOutOfScreen;
                        _visibleStartIndex = Math.Min(_visibleStartIndex, _input.Length - 1);
                    }
                    CalculateStartAndEndIndices();
                    break;
                }

                _visibleLength = indexer - _visibleStartIndex;
            }
        }

        public override string ToString()
        {
            return Buffer.ToString();
        }
    }
}
